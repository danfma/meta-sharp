namespace Metano.Annotations;

/// <summary>
/// When applied to a static class, its members are emitted as top-level exported functions
/// in the generated TypeScript module, without a wrapping class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ExportedAsModuleAttribute : Attribute;
