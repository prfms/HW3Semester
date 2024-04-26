using NUnit.Framework;
using MatrixMultiplication;

namespace MatrixMultiplicationTests;

public class MatrixTests
{
    private static IEnumerable<TestCaseData> CorrectFiles
        => new TestCaseData[]
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
    private static IEnumerable<TestCaseData> IncorrectFiles
        => new TestCaseData[]
    {
        new("../../../TestFiles/EmptyMatrix.txt"),
        new("../../../TestFiles/NotMatrix.txt")
    };

    private static IEnumerable<TestCaseData> CorrectMatrices
        => new[]
    {
        new TestCaseData(
            new Matrix(new[,]
            {
                {5, -2, 0},
                {11, 3, 3},
                {0, 56, 2}
            }),
            new Matrix(new[,]
            {
                {4, 9, 2},
                {1, 1, 1},
                {-5, -2, -1}
            }),
            new Matrix(new[,]
            {
                {18, 43, 8},
                {32, 96, 22},
                {46, 52, 54}
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
                {5, -2, 0, 2},
                {11, 3, 3, 10},
                {0, 56, 2, -7}
            }),
            new Matrix(new[,]
            {
                {4, 9, 2},
                {1, 1, 1},
                {-5, -2, -1}
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
        => Assert.That(MatrixOperation.SimpleMultiplyMatrix(firstMatrix, secondMatrix).IsEqual(expected), Is.True);

    [TestCaseSource(nameof(CorrectMatrices))]
    public void MultiplyParallel_CorrectMatrices_CorrectResultMatrix(Matrix firstMatrix, Matrix secondMatrix, Matrix expected)
        => Assert.That(MatrixOperation.ParallelMultiplyMatrix(firstMatrix, secondMatrix).IsEqual(expected), Is.True);

    [TestCaseSource(nameof(IncorrectMatrices))]
    public void Multiply_CorrectMatrices_InvalidOperationExceptionReturned(Matrix firstMatrix, Matrix secondMatrix)
        => Assert.Throws<InvalidOperationException>(() => MatrixOperation.SimpleMultiplyMatrix(firstMatrix, secondMatrix));

    [TestCaseSource(nameof(IncorrectMatrices))]
    public void MultiplyParallel_CorrectMatrices_InvalidOperationExceptionReturned(Matrix firstMatrix, Matrix secondMatrix)
        => Assert.Throws<InvalidOperationException>(() => MatrixOperation.ParallelMultiplyMatrix(firstMatrix, secondMatrix));
}
    