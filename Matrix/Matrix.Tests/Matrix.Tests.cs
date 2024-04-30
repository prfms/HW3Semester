using NUnit.Framework;

namespace Matrix.Tests;

public class MatrixTests
{
    private static IEnumerable<TestCaseData> IncorrectFiles
        => new TestCaseData[]
    {
        new("../../../TestFiles/EmptyMatrix.txt"),
        new("../../../TestFiles/NotMatrix.txt")
    };
    
    private static IEnumerable<TestCaseData> CorrectFiles
        => new[]
        {
            new TestCaseData( "../../../TestFiles/StringMatrix.txt",
                new Matrix(new[,]
                {
                    {1, 2, 3}
                })
            ),
            new TestCaseData("../../../TestFiles/VectorMatrix.txt",
                new Matrix(new[,]
                {
                    {1},
                    {2},
                    {3}
                })
            ),
            new TestCaseData("../../../TestFiles/UsualMatrix.txt",
                new Matrix(new[,]
                {
                    {1, 2, 3},
                    {1, 2, 3}
                })
            )
        };

    private static IEnumerable<TestCaseData> CorrectMatrices
        => new[]
    {
        new TestCaseData(
            new Matrix(new[,]
            {
                {1, 2, 3},
                {4, 5, 6},
                {7, 8, 9}
            }),
            new Matrix(new[,]
            {
                {1, 2, 3},
                {3, 4, 5},
                {5, 6, 7}
            }),
            new Matrix(new[,]
            {
                {22, 28, 34},
                {49, 64, 79},
                {76, 100, 124} 
            })
        ),
        new TestCaseData(
            new Matrix(new[,]
            {
                {5, -2, 0, 2},
                {11, 3, 3, 10},
                {0, 56, 2, -7}
            }),
            new Matrix(new[,]
            {
                {4, 9},
                {1, 1},
                {-5, -2},
                {2, 2}
            }),
            new Matrix(new[,]
            {
                {22, 47},
                {52, 116},
                {32, 38}
            })
        )
    };

    private static IEnumerable<TestCaseData> IncorrectMatrices
        => new[]
    {
        new TestCaseData(
            new Matrix(new[,]
            {
                {1, 1, 1, 1},
                {1, 1, 1, 1},
                {1, 1, 1, 1}
            }),
            new Matrix(new[,]
            {
                {1, 1, 1},
                {1, 1, 1},
                {1, 1, 1}
            })
        ),
    };

    [TestCaseSource(nameof(IncorrectFiles))]
    public void ReadFromFile_IncorrectFile_ArgumentExceptionReturned(string filePath)
        => Assert.Throws<InvalidDataException>(() => new Matrix(filePath));
    
    [TestCaseSource(nameof(CorrectFiles))]
    public void ReadFromFile_CorrectFile_ArgumentExceptionReturned(string filePath, Matrix expected)
        => Assert.That(new Matrix(filePath).IsEqual(expected), Is.True);

    [TestCaseSource(nameof(CorrectMatrices))]
    public void Multiply_CorrectMatrices_CorrectResultMatrix(Matrix firstMatrix, Matrix secondMatrix, Matrix expected)
        => Assert.That(MatrixMultiplication.SimpleMultiplyMatrix(firstMatrix, secondMatrix).IsEqual(expected), Is.True);

    [TestCaseSource(nameof(CorrectMatrices))]
    public void MultiplyParallel_CorrectMatrices_CorrectResultMatrix(Matrix firstMatrix, Matrix secondMatrix, Matrix expected)
        => Assert.That(MatrixMultiplication.ParallelMultiplyMatrix(firstMatrix, secondMatrix).IsEqual(expected), Is.True);

    [TestCaseSource(nameof(IncorrectMatrices))]
    public void Multiply_CorrectMatrices_InvalidOperationExceptionReturned(Matrix firstMatrix, Matrix secondMatrix)
        => Assert.Throws<InvalidOperationException>(() => MatrixMultiplication.SimpleMultiplyMatrix(firstMatrix, secondMatrix));

    [TestCaseSource(nameof(IncorrectMatrices))]
    public void MultiplyParallel_CorrectMatrices_InvalidOperationExceptionReturned(Matrix firstMatrix, Matrix secondMatrix)
        => Assert.Throws<InvalidOperationException>(() => MatrixMultiplication.ParallelMultiplyMatrix(firstMatrix, secondMatrix));
}
    