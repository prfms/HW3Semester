namespace Matrix;

/// <summary>
/// Class representing matrix.
/// </summary>
public class Matrix
{
    private int[,] _matrix;
    private static readonly Random random = new();
    
    /// <summary>
    /// Returns number of rows in the matrix.
    /// </summary>
    public int RowsCount => _matrix.GetLength(0);
    
    /// <summary>
    /// Returns number of columns in the matrix.
    /// </summary>
    public int ColumnsCount => _matrix.GetLength(1);
    
    /// <summary>
    /// Indexer for accessing elements of the matrix.
    /// </summary>
    /// <param name="i">The row index.</param>
    /// <param name="j">The column index.</param>
    /// <returns>The element at the specified row and column.</returns>
    public int this[int i, int j]
    {
        get => _matrix[i, j];
        set => _matrix[i, j] = value;
    }
    
    /// <summary>
    /// Initializes a new instance of the Matrix class with the provided matrix.
    /// </summary>
    /// <param name="matrix">The matrix data.</param>
    public Matrix(int[,] matrix) 
        =>_matrix = matrix;
    
    /// <summary>
    /// Initializes a new instance of the Matrix class
    /// with the specified number of rows and columns.
    /// </summary>
    /// <param name="rowsCount"></param>
    /// <param name="columnsCount"></param>
    public Matrix(int rowsCount, int columnsCount) 
        =>_matrix = new int[rowsCount, columnsCount];
    
    /// <summary>
    /// Initializes a new instance of the Matrix class
    /// from the data read from a file.
    /// </summary>
    /// <param name="fileName">The name of the file containing the matrix data.</param>
    /// <exception cref="InvalidDataException">Thrown when the matrix data is invalid.</exception>
    public Matrix(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        var rows = File.ReadAllLines(fileName);
        if (rows.Length == 0)
        {
            throw new InvalidDataException("File is empty.");
        }
        _matrix = new int[rows.Length, rows[0].Split(' ').Length];
        
        for (var i = 0; i < rows.Length; ++i)
        {
            var line = rows[i].Split(' ').Select(int.Parse).ToArray();

            if (line.Length != _matrix.GetLength(1))
            {
                throw new InvalidDataException("The matrix is not completely filled");
            }

            for (var j = 0; j < line.Length; ++j)
            {
                _matrix[i, j] = line[j];
            }
        }
    }
    
    /// <summary>
    /// Checks if the current matrix is equal to another matrix.
    /// </summary>
    /// <param name="otherMatrix">The matrix to compare.</param>
    /// <returns>True if the matrices are equal; otherwise, false.</returns>
    public bool IsEqual(Matrix otherMatrix)
    {
        if (RowsCount != otherMatrix.RowsCount || ColumnsCount != otherMatrix.ColumnsCount)
            return false;

        for (var i = 0; i < RowsCount; i++)
        {
            for (var j = 0; j < ColumnsCount; j++)
            {
                if (_matrix[i, j] != otherMatrix._matrix[i, j])
                    return false;
            }
        }

        return true;
    }
    
    /// <summary>
    /// Saves the matrix data to a file.
    /// </summary>
    /// <param name="fileName">The name of the file to save the matrix data to.</param>
    public void SaveMatrixToFile(string fileName)
    {
        using var writer = new StreamWriter(fileName);
        for (var i = 0; i < _matrix.GetLength(0); ++i)
        {
            for (var j = 0; j < _matrix.GetLength(1); ++j)
            {
                writer.Write($"{_matrix[i, j]} ");
            }

            writer.WriteLine();
        }
    }

    /// <summary>
    /// Generates a random matrix with the specified number of rows and columns.
    /// </summary>
    /// <param name="rowsCount">The number of rows.</param>
    /// <param name="columnsCount">The number of columns.</param>
    /// <returns>A random matrix.</returns>
    public static Matrix GenerateRandomMatrix(int rowsCount, int columnsCount)
    {
        var matrix = new int [rowsCount, columnsCount];
        for (var i = 0; i < rowsCount; ++i)
        {
            for (var j = 0; j < columnsCount; ++j)
            {
                matrix[i, j] = random.Next(-100, 101);
            }
        }
        return new Matrix(matrix);
    }
    
    /// <summary>
    /// Prints the matrix data to the console.
    /// </summary>
    public void PrintMatrix()
    {
        for (var i = 0; i < _matrix.GetLength(0); ++i)
        {
            for (var j = 0; j < _matrix.GetLength(1); ++j)
            {
                Console.Write($"{_matrix[i, j]} ");
            }

            Console.WriteLine();
        }
    }
}