namespace Matrix;

/// <summary>
/// Class represents matrix multiplication.
/// </summary>
public static class MatrixMultiplication
{
    /// <summary>
    /// Multiply two matrices sequentially.
    /// </summary>
    /// <param name="firstMatrix">The first multiplier.</param>
    /// <param name="secondMatrix">The second multiplier.</param>
    /// <returns>The product of first and second matrix.</returns>
    /// <exception cref="InvalidOperationException">Invalid matrices dimensions.</exception>
    public static Matrix SimpleMultiplyMatrix(Matrix firstMatrix, Matrix secondMatrix)
    {
        ArgumentNullException.ThrowIfNull(firstMatrix);
        ArgumentNullException.ThrowIfNull(secondMatrix);

        if (firstMatrix.ColumnsCount != secondMatrix.RowsCount)
        {
            throw new InvalidOperationException("Number of columns in first matrix must be equal to number of rows in second matrix.");
        }

        var resultMatrix = new int [firstMatrix.RowsCount, secondMatrix.ColumnsCount];
        for (var i = 0; i < firstMatrix.RowsCount; ++i)
        {
            for (var j = 0; j < secondMatrix.ColumnsCount; ++j)
            {
                for (var k = 0; k < firstMatrix.ColumnsCount; ++k)
                {
                    resultMatrix[i, j] += firstMatrix[i, k] * secondMatrix[k, j];
                }
            }
        }

        return new Matrix(resultMatrix);
    }

    /// <summary>
    /// Multiply two matrices concurrently.
    /// </summary>
    /// <param name="firstMatrix">The first multiplier.</param>
    /// <param name="secondMatrix">The second multiplier.</param>
    /// <returns>The product of first and second matrix.</returns>
    /// <exception cref="InvalidOperationException">Invalid matrices dimensions.</exception>
    public static Matrix ParallelMultiplyMatrix(Matrix firstMatrix, Matrix secondMatrix)
    {
        ArgumentNullException.ThrowIfNull(firstMatrix);
        ArgumentNullException.ThrowIfNull(secondMatrix);

        if (firstMatrix.ColumnsCount != secondMatrix.RowsCount)
        {
            throw new InvalidOperationException("Number of columns in first matrix must be equal to number of rows in second matrix.");
        }

        
        var resultMatrix = new Matrix(firstMatrix.RowsCount, secondMatrix.ColumnsCount);
        var numberOfCores = Math.Min(Environment.ProcessorCount, firstMatrix.RowsCount);
        var threads = new Thread[numberOfCores];
        
        var rowsPerThread = firstMatrix.RowsCount / numberOfCores + 1;

        for (var threadNumber = 0; threadNumber < numberOfCores; ++threadNumber)
        {
            var localThreadNumber = threadNumber;
            threads[threadNumber] = new Thread(() =>
            {
                for (var rowNumber = localThreadNumber * rowsPerThread;
                     rowNumber < (localThreadNumber + 1) * rowsPerThread && rowNumber < firstMatrix.RowsCount;
                     ++rowNumber)
                {
                    for (var columnNumber = 0; columnNumber < secondMatrix.ColumnsCount; ++columnNumber)
                    {
                        for (var k = 0; k < firstMatrix.ColumnsCount; ++k)
                        {
                            resultMatrix[rowNumber,  columnNumber] +=
                                firstMatrix[rowNumber, k] * secondMatrix[k,  columnNumber];
                        }
                    }
                }
            });
        }
        
        foreach (var thread in threads)
        {
            thread.Start();
        }
        
        foreach (var thread in threads)
        {
            thread.Join();
        }

        return resultMatrix;
    }
}

