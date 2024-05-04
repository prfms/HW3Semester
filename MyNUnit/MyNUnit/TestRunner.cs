namespace MyNUnit;
using System;
using System.Reflection;
using System.Diagnostics;
using Attributes;

/// <summary>
/// Class represents executing tests and getting results of its work.
/// </summary>
public class TestRunner
{
    /// <summary>
    /// Runs tests in all assemblies asynchronous in the specific file path.
    /// </summary>
    public async Task RunTestsAsync(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException();
        }

        var assemblies = Directory.EnumerateFiles(path, "*.dll");

        List<Task> tasksForAssemblies = [];

        foreach (var assemblyPath in assemblies)
        {
            tasksForAssemblies.Add(RunAssemblyTestsAsync(assemblyPath));
        }

        await Task.WhenAll(tasksForAssemblies);
    }

    public static async Task RunAssemblyTestsAsync(string assemblyPath)
    {
        var assembly = Assembly.LoadFrom(assemblyPath);

        var tasksForTypes = new List<Task>();
        foreach (var type in assembly.GetTypes())
        {
            tasksForTypes.Add(RunTypesTestsAsync(type));
        }

        await Task.WhenAll(tasksForTypes);
    }

    private static async Task RunTypesTestsAsync(Type type)
    {
        var instance = Activator.CreateInstance(type) ?? throw new InvalidOperationException();
        
        var methodsWithTestAttribute = type.GetMethods()
            .Where(methodInfo => 
                methodInfo.GetCustomAttributes(typeof(TestAttribute), false).Length != 0)
            .ToList();
        
        if (methodsWithTestAttribute.Count == 0) return;
        
        await CallMethodWithAttribute(type, typeof(BeforeClassAttribute));

        foreach (var method in methodsWithTestAttribute)
        {
            var testAttributes = method.GetCustomAttributes<TestAttribute>();
           
            await CallMethodWithAttribute(instance, typeof(BeforeAttribute));
            
            foreach (var attribute in testAttributes)
            {
                if (attribute.IgnoreReason != null)
                {
                    Console.WriteLine($"(!) Test '{method.Name}' ignored: {attribute.IgnoreReason}");
                    continue;
                }

                if (attribute.Expected != null)
                {
                    Console.WriteLine($"(!) Test '{method.Name}' expected failure: {attribute.Expected}");
                }
                
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                
                try
                {
                    await Task.Run(() => method.Invoke(instance, null));
                    
                    Console.WriteLine($"(+) Test '{method.Name}' passed");
                }
                catch (Exception ex)
                {
                    if (attribute.Expected != null && attribute.Expected == ex.InnerException?.GetType())
                    {
                        Console.WriteLine($"(+) Test '{method.Name}' failed as expected: {ex.InnerException?.Message}");
                    }
                    else
                    {
                        Console.WriteLine($"(-) Test '{method.Name}' failed unexpectedly: {ex.InnerException?.Message}");
                    }
                }
                
                stopwatch.Stop();
                var testTime = stopwatch.Elapsed.TotalMilliseconds;
                Console.WriteLine($"(T) Testing took {testTime} ms.");
            }
            await CallMethodWithAttribute(instance, typeof(AfterAttribute));
        }
        await CallMethodWithAttribute(type, typeof(AfterClassAttribute));
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

