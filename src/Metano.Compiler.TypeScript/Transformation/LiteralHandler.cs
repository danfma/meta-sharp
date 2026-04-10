using Metano.TypeScript.AST;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metano.Transformation;

/// <summary>
/// Lowers C# literal expressions (<c>"text"</c>, <c>42</c>, <c>true</c>, <c>null</c>,
/// <c>default</c>) into the corresponding TypeScript literal AST nodes.
///
/// Notes on individual mappings:
/// <list type="bullet">
///   <item><c>null</c> and <c>default</c> both lower to <c>null</c> — see the user's
///   preference recorded in memory for keeping <c>T | null = null</c> instead of
///   converting to <c>T?</c>.</item>
///   <item>Numeric literals are normally forwarded by their token's <c>ValueText</c>,
///   which already strips the C# numeric suffixes (<c>m</c>, <c>L</c>, <c>f</c>,
///   <c>d</c>). When the semantic type is <c>System.Decimal</c>, the literal is wrapped
///   in <c>new Decimal("…")</c> instead so that <c>decimal.js</c> can preserve the
///   exact value (passing the raw number through the JS <c>Decimal</c> constructor
///   would already lose precision).</item>
/// </list>
/// </summary>
public static class LiteralHandler
{
    public static TsExpression Transform(LiteralExpressionSyntax lit, SemanticModel? model = null)
    {
        return lit.Kind() switch
        {
            SyntaxKind.StringLiteralExpression => new TsStringLiteral(lit.Token.ValueText),
            SyntaxKind.TrueLiteralExpression => new TsLiteral("true"),
            SyntaxKind.FalseLiteralExpression => new TsLiteral("false"),
            SyntaxKind.NullLiteralExpression => new TsLiteral("null"),
            SyntaxKind.DefaultLiteralExpression => new TsLiteral("null"),
            SyntaxKind.NumericLiteralExpression => TransformNumeric(lit, model),
            _ => new TsLiteral(lit.Token.ValueText),
        };
    }

    private static TsExpression TransformNumeric(LiteralExpressionSyntax lit, SemanticModel? model)
    {
        // For decimal literals (1.5m), wrap the literal in `new Decimal("…")` so that
        // decimal.js preserves the exact value. We pass the value as a string because
        // converting through a JS number first would already lose precision.
        if (
            model is not null
            && model.GetTypeInfo(lit).Type?.SpecialType == SpecialType.System_Decimal
        )
        {
            return new TsNewExpression(
                new TsIdentifier("Decimal"),
                [new TsStringLiteral(lit.Token.ValueText)]
            );
        }
        return new TsLiteral(lit.Token.ValueText);
    }
}
