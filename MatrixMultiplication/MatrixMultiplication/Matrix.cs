namespace MatrixMultiplication;

public class Matrix
{
    public readonly int[,] _matrix;
    private static readonly Random Rand = new Random();
    public int RowsCount => _matrix.GetLength(0);
    public int ColumnsCount => _matrix.GetLength(1);

    public Matrix(int[,] matrix)
    {
        this._matrix = matrix;
    }
    public Matrix(int rowsCount, int columnsCount)
    {
        _matrix = new int[rowsCount, columnsCount];
    }
    
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
    
    public bool IsEqual(Matrix otherMatrix)
    {
        if (otherMatrix == null)
            return false;

        if (ReferenceEquals(this, otherMatrix))
            return true;

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

    public static Matrix GenerateRandomMatrix(int rowsCount, int columnsCount)
    {
        var matrix = new int [rowsCount, columnsCount];
        for (var i = 0; i < rowsCount; ++i)
        {
            for (var j = 0; j < columnsCount; ++j)
            {
                matrix[i, j] = Rand.Next(-100, 101);
            }
        }
        return new Matrix(matrix);
    }
    
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