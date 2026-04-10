namespace Metano.Annotations;

/// <summary>
/// Makes an enum transpile as a TypeScript string union type instead of a numeric enum.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public sealed class StringEnumAttribute : Attribute;
