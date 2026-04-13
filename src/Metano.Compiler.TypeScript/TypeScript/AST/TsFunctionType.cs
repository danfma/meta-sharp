namespace Metano.TypeScript.AST;

/// <summary>
/// A TypeScript function type: <c>(param: T) => R</c>.
/// Used for delegate type mappings (<c>Action&lt;T&gt;</c>, <c>Func&lt;T,R&gt;</c>,
/// <c>EventHandler&lt;T&gt;</c>, and custom delegate types).
/// </summary>
public sealed record TsFunctionType(IReadOnlyList<TsParameter> Parameters, TsType ReturnType)
    : TsType;
