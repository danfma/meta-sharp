namespace Metano.TypeScript.AST;

public sealed record TsThrowStatement(TsExpression Expression) : TsStatement;
