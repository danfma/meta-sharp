namespace Metano.Annotations;

/// <summary>
/// Excludes a member from transpilation.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class IgnoreAttribute : Attribute;
