using System.Runtime.CompilerServices;

namespace MyNUnit;
using System;
using System.Reflection;
using System.Diagnostics;
using Attributes;

/// <summary>
/// Class represents executing tests and getting results of its work.
/// </summary>
public static class TestRunner
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

        var tasksForAssemblies = new List<Task<List<TestResultInfo>>>();

        foreach (var assemblyPath in assemblies)
        {
            tasksForAssemblies.Add(RunAssemblyTestsAsync(assemblyPath));
        }

        var results = await Task.WhenAll(tasksForAssemblies);
        
        foreach (var resultList in results)
        {
            TestResultInfo.PrintResults(resultList, writer);
        }
    }
    
    /// <summary>
    /// Runs all tests in the assembly.
    /// </summary>
    /// <param name="assemblyPath">Assembly where the tests are executed.</param>
    /// <param name="writer">Source to write results in.</param>
    public static async Task<List<TestResultInfo>> RunAssemblyTestsAsync(string assemblyPath)
    {
        var assembly = Assembly.LoadFrom(assemblyPath);

        var tasksForTypes = new List<Task<List<TestResultInfo>>>();
        foreach (var type in assembly.GetTypes())
        {
            tasksForTypes.Add(RunTypesTestsAsync(type));
        }

        var results = await Task.WhenAll(tasksForTypes);

        var joinedResults = new List<TestResultInfo>();
        foreach (var result in results)
        {
            joinedResults = joinedResults.Concat(result).ToList();
        }
        return joinedResults;
    }

    private static async Task<List<TestResultInfo>> RunTypesTestsAsync(Type type)
    {
        await CallMethodWithAttribute(type, typeof(BeforeClassAttribute));
        
        var methodsWithTestAttribute = type.GetMethods()
            .Where(methodInfo => 
                methodInfo.GetCustomAttributes(typeof(TestAttribute), false).Length != 0)
            .ToList();
        
        if (methodsWithTestAttribute.Count == 0) return [];

        var tasksTestMethod = new List<Task<TestResultInfo>>();
        foreach (var method in methodsWithTestAttribute)
        {
            if (ValidateTestMethod(method))
            {
                tasksTestMethod.Add(RunMethodsAsync(type, method));
            }
        }
        
        var result = await Task.WhenAll(tasksTestMethod);
        
        await CallMethodWithAttribute(type, typeof(AfterClassAttribute));
        
        return result.ToList();;
    }

    private static async Task<TestResultInfo> RunMethodsAsync(Type type, MethodInfo method)
    {
        var isPassed = true;
        var failedExpected = false;
        var duration = 0.0;
        var exceptionMessage = "";
        var ignoreMessage = "";
        var unexpectedException = "";

        var testAttributes = method.GetCustomAttributes<TestAttribute>();

        await CallMethodWithAttribute(type, typeof(BeforeAttribute));
            
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

            var instance = Activator.CreateInstance(type);
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                method.Invoke(instance, null);
            }
            catch (Exception ex)
            {
                if (attribute.Expected != null && attribute.Expected == ex.InnerException?.GetType())
                {
                    failedExpected = true;
                }
                else
                {
                    isPassed = false;
                    unexpectedException = ex.InnerException?.Message;
                }
            }

            stopwatch.Stop();
            duration = stopwatch.ElapsedMilliseconds;
        }

        await CallMethodWithAttribute(type, typeof(AfterAttribute));
        
        return new TestResultInfo(method.Name, isPassed, failedExpected, duration, exceptionMessage,
            ignoreMessage, unexpectedException);
    }

    private static bool ValidateTestMethod(MethodInfo method)
    {
        return method.GetParameters().Length == 0 && method.ReturnType == typeof(void);
    }
    
    private static async Task CallMethodWithAttribute(Type type, Type attributeType)
    {
        foreach (var method in type.GetMethods())
        {
            if (method.GetCustomAttributes(attributeType, false).Length > 0)
            {
                if (attributeType == typeof(AfterClassAttribute) || attributeType == typeof(BeforeClassAttribute))
                {
                    await Task.Run(() => method.Invoke(null, null));
                }
                else
                {
                    var instance = Activator.CreateInstance(type) ?? throw new InvalidOperationException();
                    await Task.Run(() => method.Invoke(instance, null));
                }
            }
        }
    }
    
}

