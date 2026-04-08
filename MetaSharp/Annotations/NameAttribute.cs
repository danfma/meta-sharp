namespace MetaSharp.Annotations;

/// <summary>
/// Overrides the name used in the TypeScript output.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class NameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
