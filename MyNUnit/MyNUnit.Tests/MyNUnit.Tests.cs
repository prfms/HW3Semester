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
        
        var actualRecords = await TestRunner.RunAssemblyTestsAsync(assemblyPath);
        
        Assert.Multiple(() =>
        {
            Assert.That(actualRecords[0].TestName, Is.EqualTo("PrintSomeText"));
            Assert.That(actualRecords[0].IsPassed, Is.EqualTo(true));
            Assert.That(actualRecords[1].TestName, Is.EqualTo("TestDivideByZero"));
            Assert.That(actualRecords[1].IsPassed, Is.EqualTo(true));
            Assert.That(actualRecords[1].FailedExpected, Is.EqualTo(true));
            Assert.That(actualRecords[2].TestName, Is.EqualTo("TestNotImplemented"));
            Assert.That(actualRecords[2].IgnoreMessage, Is.EqualTo("This test is not implemented yet"));
            Assert.That(actualRecords[2].IsPassed, Is.EqualTo(true));
        });
    }

    [Test]
    public async Task TestRunner_RunsBeforeAndAfterAttributes()
    {
        const string assemblyPath = @"..\..\..\..\TestProject\bin\Debug\net8.0\TestProject.dll";
        var assembly = Assembly.LoadFrom(assemblyPath);
        
        await TestRunner.RunAssemblyTestsAsync(assemblyPath);
        
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