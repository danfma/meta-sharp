namespace Metano.TypeScript.AST;

public sealed record TsPropertyAccess(TsExpression Object, string Property) : TsExpression;
