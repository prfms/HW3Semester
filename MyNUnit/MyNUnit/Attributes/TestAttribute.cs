namespace MyNUnit.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class TestAttribute(Type? expected = null, string? ignoreReason = null) : Attribute
{ 
    public Type? Expected { get; set; } = expected;
    public string? IgnoreReason { get; set; } = ignoreReason;
}
