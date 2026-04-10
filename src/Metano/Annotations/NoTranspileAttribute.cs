namespace Metano.Annotations;

/// <summary>
/// Excludes a type from transpilation even when [assembly: TranspileAssembly] is used.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface)]
public sealed class NoTranspileAttribute : Attribute;
