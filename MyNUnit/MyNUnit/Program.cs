using MyNUnit;

if (args.Length == 0 || args[0] == "-help")
{
    Console.WriteLine("""

                      This program runs tests for all assemblies in specific path.
                      - - - - -- - - - - - - - - - - - - - - - - - - - - - - - - - -
                      To run tests:
                      - - - - -- - - - - - - - - - - - - - - - - - - - - - - - - - -
                      dotnet run [path]
                      - - - - -- - - - - - - - - - - - - - - - - - - - - - - - - - -
                      
                      """);

    return 0;
}

if (args.Length == 1)
{
    var path = args[0];
    var testRunner = new TestRunner();
    await testRunner.RunTestsAsync(path);
}
else
{
    Console.WriteLine("Invalid command. Try again!\n For help use command <dotnet run -help>");
}
return 0;
/*
class Program
{
    static async Task Main()
    {
        var path = "C:\\Users\\nasty\\RiderProjects\\HWs3Semester\\MyNUnit\\TestProject\\bin\\Debug\\net8.0\\TestProject.dll";
        var testRunner = new TestRunner();
        //await testRunner.RunTestsAsync(path);
        await TestRunner.RunAssemblyTestsAsync(path);
    }
}
*/