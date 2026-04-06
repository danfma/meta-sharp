namespace MetaSharp.TypeScript.AST;

/// <summary>
/// A TypeScript namespace declaration containing exported functions.
/// Used for InlineWrapper companion namespaces.
/// </summary>
public sealed record TsNamespaceDeclaration(
    string Name,
    IReadOnlyList<TsFunction> Functions,
    bool Exported = true
) : TsTopLevel;
