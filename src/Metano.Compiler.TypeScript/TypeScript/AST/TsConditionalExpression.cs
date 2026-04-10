namespace Metano.TypeScript.AST;

public sealed record TsConditionalExpression(
    TsExpression Condition,
    TsExpression WhenTrue,
    TsExpression WhenFalse
) : TsExpression;
