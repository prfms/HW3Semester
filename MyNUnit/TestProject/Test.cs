namespace TestProject;
using MyNUnit.Attributes;

public class Test
{
    public static volatile int beforeCount;
    public static volatile int afterCount;
    public static volatile int afterClassCount;
    public static volatile int beforeClassCount;
    
    [BeforeClassAttribute]
    public static void BeforeClass()
    {
        Interlocked.Increment(ref beforeClassCount);
        Console.WriteLine("BeforeClass actions were made.");
    }

    [BeforeAttribute]
    public void Before()
    {
        Interlocked.Increment(ref beforeCount);
        Console.WriteLine("Before actions were made.");
    }
    
    [AfterAttribute]
    public void After()
    {
        Interlocked.Increment(ref afterCount);
        Console.WriteLine("After actions were made.");
    }

    [AfterClassAttribute]
    public static void AfterClass()
    {
        Interlocked.Increment(ref afterClassCount);
        Console.WriteLine("AfterClass actions were made.");
    }
    
    [TestAttribute]
    public void PrintSomeText()
    {
        Console.WriteLine("Everything is OK:)");
    }
    
    [TestAttribute(Expected = typeof(DivideByZeroException))]
    public void TestDivideByZero()
    {
        var zero = 0;
        var number = 10;
        var result = number / zero;
    }

    [TestAttribute(IgnoreReason = "This test is not implemented yet")]
    public void TestNotImplemented()
    {
        Console.WriteLine("This messages should not be shown!");
    }
}