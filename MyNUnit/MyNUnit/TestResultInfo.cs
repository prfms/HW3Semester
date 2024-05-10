namespace MyNUnit;

/// <summary>
/// Record to store information about test execution.
/// </summary>
/// <param name="TestName">Name of the test.</param>
/// <param name="IsPassed">Shows if test is passed.</param>
/// <param name="FailedExpected">Shows if test expects failure.</param>
/// <param name="Duration">Shows the duration of test execution.</param>
/// <param name="Expected">Message which the failure is expected with.</param>
/// <param name="IgnoreMessage">Message showing why the test was ignored.</param>
/// <param name="UnexpectedException">Unexpected failure message.</param>
public record TestResultInfo(
    string TestName,
    bool IsPassed,
    bool FailedExpected,
    double Duration,
    string? Expected,
    string? IgnoreMessage,
    string? UnexpectedException)
{
    /// <summary>
    /// Print results of test execution.
    /// </summary>
    /// <param name="testResults">List of test results records.</param>
    /// <param name="writer">Source where the results are written.</param>
    public static void PrintResults(List<TestResultInfo> testResults, TextWriter writer)
    {
        foreach (var test in testResults)
        {
            if (test.IgnoreMessage != "")
            {
                writer.WriteLine($"(!) Test '{test.TestName}' ignored: {test.IgnoreMessage}");
            }

            if (test.Expected != "")
            {
                writer.WriteLineAsync(
                    test.FailedExpected
                        ? $"(+) Test '{test.TestName}' failed as expected: {test.Expected}"
                        : $"(-) Test '{test.TestName}' failed unexpectedly: {test.UnexpectedException}");
            }

            if (test.IsPassed)
            {
                writer.WriteLineAsync($"(+) Test '{test.TestName}' passed");
            }
            
            writer.WriteLineAsync($"(T) Testing took {test.Duration} ms.");
        }
    }
}
    
