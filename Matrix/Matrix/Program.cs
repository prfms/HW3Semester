using Matrix;

if (args.Length == 0 || args[0] == "-help")
{
    Console.WriteLine("""

              This program for matrix multiplication using concurrent computing.

              To multiply matrices from files:
              - - - - -- - - - - - - - - - - - - - - - - - - - - - - - - - -
              dotnet run [first matrix file path] [second matrix file path]
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
            Console.WriteLine("Failed.");
            Console.WriteLine(e.Message);
        }
    }
    else
    {
        Console.WriteLine("Unknown command");
        Console.WriteLine("For help, use the command: dotnet run -help");
    }
}
if (args.Length == 2)
{
    try
    {
        var firstMatrix = new Matrix.Matrix(args[0]);
        var secondMatrix = new Matrix.Matrix(args[1]);

        var resultMatrix = MatrixMultiplication.ParallelMultiplyMatrix(firstMatrix, secondMatrix);
        resultMatrix.SaveMatrixToFile("ResultMatrix.txt");
    }
    catch (Exception ex) when (ex is ArgumentNullException or InvalidDataException)
    {
        Console.WriteLine(ex.Message);
    }
}
else
{
    Console.WriteLine("Unknown command");
    Console.WriteLine("For help, use the command: dotnet run -help");
}


return 0;