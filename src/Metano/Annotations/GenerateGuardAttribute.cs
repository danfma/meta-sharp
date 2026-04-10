namespace Metano.Annotations;

/// <summary>
/// Generates a type guard function for this type.
/// The guard validates runtime values using instanceof + shape checking.
/// </summary>
[AttributeUsage(
    AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Enum
        | AttributeTargets.Interface
)]
public sealed class GenerateGuardAttribute : Attribute;
