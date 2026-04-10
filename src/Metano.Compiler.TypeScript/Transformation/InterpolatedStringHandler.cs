using Metano.TypeScript.AST;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metano.Transformation;

/// <summary>
/// Lowers C# interpolated strings (<c>$"Hello {name}!"</c>) into TypeScript template
/// literals (<c>`Hello ${name}!`</c>).
///
/// Walks the interpolation contents in order, accumulating literal text into the
/// <c>quasis</c> list and forwarding interpolated expressions to the parent
/// <see cref="ExpressionTransformer"/> for recursive transformation.
/// </summary>
public sealed class InterpolatedStringHandler(ExpressionTransformer parent)
{
    private readonly ExpressionTransformer _parent = parent;

    public TsExpression Transform(InterpolatedStringExpressionSyntax interp)
    {
        var quasis = new List<string>();
        var expressions = new List<TsExpression>();
        var current = "";

        foreach (var content in interp.Contents)
        {
            switch (content)
            {
                case InterpolatedStringTextSyntax text:
                    current += text.TextToken.ValueText;
                    break;

                case InterpolationSyntax interpolation:
                    quasis.Add(current);
                    current = "";
                    expressions.Add(_parent.TransformExpression(interpolation.Expression));
                    break;
            }
        }

        quasis.Add(current);
        return new TsTemplateLiteral(quasis, expressions);
    }
}
