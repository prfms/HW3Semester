using MatrixMultiplication;
using static System.Console;
using Matrix = MatrixMultiplication.Matrix;


if (args.Length == 0 || args[0] == "-help")
{
    WriteLine("""

              This program for _matrix multiplication using parallel computing.

              To multiply matrices from files:
              - - - - -- - - - - - - - - - - - - - - - - - - - - - - - - - -
              dotnet run [first _matrix file path] [second _matrix file path]
              - - - - -- - - - - - - - - - - - - - - - - - - - - - - - - - -

              To see statistics:
              - - - - -- - - - - - - - - - - - - - - - - - - - - - - - - - -
              dotnet run -reports
              - - - - -- - - - - - - - - - - - - - - - - - - - - - - - - - -

              """);

    return 0;
}

if (args.Length == 1)
{
    if (args[0] == "-reports")
    {
        try
        {
            var reports = Statistics.CreateReports();
            Statistics.SaveReportsToFile(reports, "Reports.pdf");
        }
        catch (IOException e)
        {
            WriteLine("Failed.");
            WriteLine(e.Message);
        }
    }
    else
    {
        WriteLine("Unknown command");
        WriteLine("For help, use the command: dotnet run -help");
    }
}
else
{
    try
    {
        var firstMatrix = new Matrix(args[0]);
        var secondMatrix = new Matrix(args[1]);

        var resultMatrix = MatrixOperation.ParallelMultiplyMatrix(firstMatrix, secondMatrix);
        resultMatrix.SaveMatrixToFile("ResultMatrix.txt");
    }
    catch (Exception ex) when (ex is ArgumentNullException or InvalidDataException)
    {
        WriteLine(ex.Message);
    }
}


return 0;