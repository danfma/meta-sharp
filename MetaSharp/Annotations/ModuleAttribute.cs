namespace MetaSharp.Annotations;

/// <summary>
/// Marks a static class to be transpiled as a TypeScript module (exported functions).
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ModuleAttribute : Attribute;
