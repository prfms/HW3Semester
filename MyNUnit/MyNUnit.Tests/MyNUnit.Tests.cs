using NUnit.Framework;
namespace MyNUnit.Tests;
using MyNUnit;
using System.Reflection;
using System.Text;

public class MyNUnitTests
{
    [Test]
    public async Task TestRunnerRunsAllTestsAndAttributes()
    { 
        const string assemblyPath = @"..\..\..\..\TestProject\bin\Debug\net8.0\TestProject.dll";
        var stringBuilder = new StringBuilder();
        var writer = new StringWriter(stringBuilder);
        await TestRunner.RunAssemblyTestsAsync(assemblyPath, writer);
        
        var reader = new StringReader(stringBuilder.ToString());
        var actualOutput = await reader.ReadToEndAsync();
        var actualOutputArray = actualOutput.Split('\n');
        
        Assert.Multiple(() =>
        {
            Assert.That(actualOutputArray[0], Is.EqualTo("(+) Test 'PrintSomeText' passed\r"));
            Assert.That(actualOutputArray[2], Is.EqualTo("(+) Test 'TestDivideByZero' failed as expected: System.DivideByZeroException\r"));
            Assert.That(actualOutputArray[4],Is.EqualTo("(!) Test 'TestNotImplemented' ignored: This test is not implemented yet\r"));
            });
    }

    [Test]
    public async Task TestRunner_RunsBeforeAndAfterAttributes()
    {
        const string assemblyPath = @"..\..\..\..\TestProject\bin\Debug\net8.0\TestProject.dll";
        var assembly = Assembly.LoadFrom(assemblyPath);
        
        var stringBuilder = new StringBuilder();
        var writer = new StringWriter(stringBuilder);
        await TestRunner.RunAssemblyTestsAsync(assemblyPath, writer);
        
        var test = assembly.GetTypes().First(t => t.Name == "Test");
        var beforeCount = test.GetField("beforeCount")!.GetValue(null);
        var beforeClassCount = test.GetField("beforeClassCount")!.GetValue(null);
        var afterCount = test.GetField("afterCount")!.GetValue(null);
        var afterClassCount = test.GetField("afterClassCount")!.GetValue(null);
        
        Assert.Multiple(() =>
        {
            Assert.That(beforeCount, Is.EqualTo(3));
            Assert.That(afterCount, Is.EqualTo(3));
            Assert.That(beforeClassCount, Is.EqualTo(1));
            Assert.That(afterClassCount, Is.EqualTo(1));
        });
    }
}