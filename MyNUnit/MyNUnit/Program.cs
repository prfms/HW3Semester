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
    await TestRunner.RunTestsAsync(path, Console.Out);
}
else
{
    Console.WriteLine("Invalid command. Try again!\n For help use command <dotnet run -help>");
}
return 0;

