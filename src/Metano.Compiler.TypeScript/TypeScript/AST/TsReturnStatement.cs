namespace Metano.TypeScript.AST;

public sealed record TsReturnStatement(TsExpression? Expression = null) : TsStatement;
