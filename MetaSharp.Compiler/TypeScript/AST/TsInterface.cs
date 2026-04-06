namespace MetaSharp.TypeScript.AST;

public sealed record TsInterface(
    string Name,
    IReadOnlyList<TsProperty> Properties,
    bool Exported = true,
    IReadOnlyList<TsTypeParameter>? TypeParameters = null
) : TsTopLevel;
