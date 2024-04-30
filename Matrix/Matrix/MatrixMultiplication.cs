namespace Matrix;

/// <summary>
/// Class represents matrix multiplication.
/// </summary>
public static class MatrixOperation
{
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
                    resultMatrix[i, j] += firstMatrix.GetElement(i, k) * secondMatrix.GetElement(k, j);
                }
            }
        }

        return new Matrix(resultMatrix);
    }

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
            var nestedThreadNumber = threadNumber;
            threads[threadNumber] = new Thread(() =>
            {
                for (var rowNumber = nestedThreadNumber * rowsPerThread;
                     rowNumber < (nestedThreadNumber + 1) * rowsPerThread && rowNumber < firstMatrix.RowsCount;
                     ++rowNumber)
                {
                    for (var columnNumber = 0; columnNumber < secondMatrix.ColumnsCount; ++columnNumber)
                    {
                        var nestedRow = rowNumber;
                        var nestedColumn = columnNumber;

                        resultMatrix.SetElement(nestedRow, nestedColumn, Enumerable.Range(0, firstMatrix.ColumnsCount)
                            .Sum(k => firstMatrix.GetElement(nestedRow, k)* secondMatrix.GetElement(k, nestedColumn)));
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

