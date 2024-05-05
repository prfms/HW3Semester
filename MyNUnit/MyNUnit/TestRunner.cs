namespace MyNUnit;
using System;
using System.Reflection;
using System.Diagnostics;
using Attributes;

/// <summary>
/// Class represents executing tests and getting results of its work.
/// </summary>
public abstract class TestRunner
{
    /// <summary>
    /// Runs tests in all assemblies asynchronous in the specific file path.
    /// </summary>
    public static async Task RunTestsAsync(string path, TextWriter writer)
    {
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException();
        }

        var assemblies = Directory.EnumerateFiles(path, "*.dll");

        var tasksForAssemblies = new List<Task>();

        foreach (var assemblyPath in assemblies)
        {
            tasksForAssemblies.Add(RunAssemblyTestsAsync(assemblyPath, writer));
        }

        await Task.WhenAll(tasksForAssemblies);
    }
    
    /// <summary>
    /// Runs all tests in the assembly.
    /// </summary>
    /// <param name="assemblyPath">Assembly where the tests are executed.</param>
    /// <param name="writer">Source to write results in.</param>
    public static async Task RunAssemblyTestsAsync(string assemblyPath, TextWriter writer)
    {
        var assembly = Assembly.LoadFrom(assemblyPath);

        var tasksForTypes = new List<Task<List<TestResultInfo>>>();
        foreach (var type in assembly.GetTypes())
        {
            tasksForTypes.Add(RunTypesTestsAsync(type));
        }

        var results = await Task.WhenAll(tasksForTypes);
        foreach (var resultList in results)
        {
            TestResultInfo.PrintResults(resultList, writer);
        }
    }

    private static async Task<List<TestResultInfo>> RunTypesTestsAsync(Type type)
    {
        var instance = Activator.CreateInstance(type) ?? throw new InvalidOperationException();
        
        var methodsWithTestAttribute = type.GetMethods()
            .Where(methodInfo => 
                methodInfo.GetCustomAttributes(typeof(TestAttribute), false).Length != 0)
            .ToList();
        
        if (methodsWithTestAttribute.Count == 0) return [];
        
        await CallMethodWithAttribute(type, typeof(BeforeClassAttribute));

        var resultList = new List<TestResultInfo>();

        foreach (var method in methodsWithTestAttribute)
        {
            var isPassed = false;
            var failedExpected = false;
            var duration = 0.0;
            var exceptionMessage = "";
            var ignoreMessage = "";
            var unexpectedException = "";

            var testAttributes = method.GetCustomAttributes<TestAttribute>();

            await CallMethodWithAttribute(instance, typeof(BeforeAttribute));

            foreach (var attribute in testAttributes)
            {
                if (attribute.IgnoreReason != null)
                {
                    ignoreMessage = attribute.IgnoreReason;
                    continue;
                }

                if (attribute.Expected != null)
                {
                    exceptionMessage = attribute.Expected.ToString();
                }

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                try
                {
                    method.Invoke(instance, null);
                    isPassed = true;
                }
                catch (Exception ex)
                {
                    if (attribute.Expected != null && attribute.Expected == ex.InnerException?.GetType())
                    {
                        failedExpected = true;
                    }
                    else
                    {
                        unexpectedException = ex.InnerException?.Message;
                    }
                }

                stopwatch.Stop();
                duration = stopwatch.Elapsed.TotalMilliseconds;
            }

            await CallMethodWithAttribute(instance, typeof(AfterAttribute));

            resultList.Add(new TestResultInfo(method.Name, isPassed, failedExpected, duration, exceptionMessage,
                ignoreMessage, unexpectedException));
        }

        await CallMethodWithAttribute(type, typeof(AfterClassAttribute));
        return resultList;
    }

    private static async Task CallMethodWithAttribute(Type type, Type attributeType)
    {
        foreach (var method in type.GetMethods())
        {
            if (method.GetCustomAttributes(attributeType, false).Length > 0)
            {
                await Task.Run(() => method.Invoke(null, null));
            }
        }
    }

    private static async Task CallMethodWithAttribute(object instance, Type attributeType)
    {
        foreach (var method in instance.GetType().GetMethods())
        {
            if (method.GetCustomAttributes(attributeType, false).Length > 0)
            {
                await Task.Run(() => method.Invoke(instance, null));
            }
        }
    }
}

