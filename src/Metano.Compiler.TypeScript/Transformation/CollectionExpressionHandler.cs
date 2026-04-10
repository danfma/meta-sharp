using Metano.TypeScript.AST;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metano.Transformation;

/// <summary>
/// Handles C# 12 collection expressions (<c>[]</c>, <c>[a, b, c]</c>) and lowers them
/// to either an array literal / <c>Array.of(...)</c> call or a <c>new HashSet([...])</c>
/// depending on the converted target type.
///
/// The target type is read from the parent <see cref="ExpressionTransformer"/>'s
/// semantic model: when it resolves to <c>HashSet</c>, <c>ISet</c>, or <c>SortedSet</c>,
/// the output uses the runtime <see cref="HashSet"/> implementation; otherwise the
/// elements become a TypeScript array.
/// </summary>
public sealed class CollectionExpressionHandler(ExpressionTransformer parent)
{
    private readonly ExpressionTransformer _parent = parent;

    public TsExpression Transform(CollectionExpressionSyntax collExpr)
    {
        // Check target type to distinguish Set vs Array
        var convertedType = _parent.Model.GetTypeInfo(collExpr).ConvertedType;
        var isSetType =
            convertedType is INamedTypeSymbol named
            && named.Name is "HashSet" or "ISet" or "SortedSet";

        if (collExpr.Elements.Count == 0)
            return isSetType
                ? new TsNewExpression(new TsIdentifier("HashSet"), [])
                : new TsLiteral("[]");

        var elements = collExpr
            .Elements.OfType<ExpressionElementSyntax>()
            .Select(e => _parent.TransformExpression(e.Expression))
            .ToList();

        if (isSetType)
            return new TsNewExpression(new TsIdentifier("HashSet"), [new TsArrayLiteral(elements)]);

        return new TsCallExpression(
            new TsPropertyAccess(new TsIdentifier("Array"), "of"),
            elements
        );
    }
}
