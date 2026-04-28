namespace Metano.Annotations;

/// <summary>
/// Marks a type for transpilation to TypeScript.
/// </summary>
[AttributeUsage(
    AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Enum
        | AttributeTargets.Interface
        | AttributeTargets.Delegate
)]
public sealed class TranspileAttribute : Attribute;
