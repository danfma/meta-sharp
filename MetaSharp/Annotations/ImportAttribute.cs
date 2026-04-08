namespace MetaSharp.Annotations;

/// <summary>
/// Declares that a type or member is imported from an external JavaScript module.
/// The type body is not transpiled — only the import statement is generated.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property)]
public sealed class ImportAttribute(string name, string from) : Attribute
{
    public string Name { get; } = name;
    public string From { get; } = from;
}
