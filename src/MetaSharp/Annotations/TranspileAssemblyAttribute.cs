namespace MetaSharp.Annotations;

/// <summary>
/// Marks all public types in the assembly for transpilation.
/// Use [NoTranspile] on specific types to opt out.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class TranspileAssemblyAttribute : Attribute;
