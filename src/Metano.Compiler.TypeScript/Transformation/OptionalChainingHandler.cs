using Metano.TypeScript.AST;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metano.Transformation;

/// <summary>
/// Lowers C# null-conditional operator expressions (<c>x?.Prop</c>, <c>x?.Method()</c>)
/// into TypeScript optional chaining (<c>x?.prop</c>, <c>x?.method()</c>).
///
/// Two shapes are recognised:
/// <list type="bullet">
///   <item>Member binding (<c>x?.Prop</c>) → <see cref="TsPropertyAccess"/> with the
///   receiver suffixed by <c>?</c>.</item>
///   <item>Invocation member binding (<c>x?.Method(args)</c>) → the same property access
///   wrapped in a <see cref="TsCallExpression"/>.</item>
/// </list>
///
/// The receiver text composition is delegated to <see cref="GetExpressionText"/>, a tiny
/// helper that walks identifiers and dotted accesses recursively.
/// </summary>
public sealed class OptionalChainingHandler(ExpressionTransformer parent)
{
    private readonly ExpressionTransformer _parent = parent;

    public TsExpression Transform(ConditionalAccessExpressionSyntax condAccess)
    {
        var obj = _parent.TransformExpression(condAccess.Expression);

        return condAccess.WhenNotNull switch
        {
            // x?.Prop → x?.prop
            MemberBindingExpressionSyntax memberBinding => new TsPropertyAccess(
                new TsIdentifier(GetExpressionText(obj) + "?"),
                TypeScriptNaming.ToCamelCase(memberBinding.Name.Identifier.Text)
            ),

            // x?.Method() → x?.method()
            InvocationExpressionSyntax
            {
                Expression: MemberBindingExpressionSyntax binding
            } invocation => new TsCallExpression(
                new TsPropertyAccess(
                    new TsIdentifier(GetExpressionText(obj) + "?"),
                    TypeScriptNaming.ToCamelCase(binding.Name.Identifier.Text)
                ),
                invocation
                    .ArgumentList.Arguments.Select(a => _parent.TransformExpression(a.Expression))
                    .ToList()
            ),

            _ => obj, // fallback
        };
    }

    /// <summary>
    /// Gets a simple text representation of an expression for optional chaining
    /// composition. Recurses through dotted property accesses and bottoms out at
    /// identifiers; anything else collapses to the literal string <c>"unknown"</c>.
    /// </summary>
    private static string GetExpressionText(TsExpression expr) =>
        expr switch
        {
            TsIdentifier id => id.Name,
            TsPropertyAccess access => GetExpressionText(access.Object) + "." + access.Property,
            _ => "unknown",
        };
}
