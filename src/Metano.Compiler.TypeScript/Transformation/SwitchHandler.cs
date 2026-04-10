using Metano.TypeScript.AST;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metano.Transformation;

/// <summary>
/// Handles C# <c>switch</c> statements and <c>switch</c> expressions and lowers them
/// into the equivalent TypeScript constructs.
///
/// <list type="bullet">
///   <item>
///     <c>switch (e) { case A: ... }</c> → <see cref="TsSwitchStatement"/> with one
///     <see cref="TsSwitchCase"/> per case label. Pattern-based labels currently
///     fall through to the default branch.
///   </item>
///   <item>
///     <c>e switch { p1 =&gt; v1, p2 =&gt; v2, _ =&gt; def }</c> → a chained ternary
///     <c>cond1 ? v1 : cond2 ? v2 : def</c> built recursively. Each pattern condition
///     is produced by <see cref="PatternMatchingHandler"/>.
///   </item>
/// </list>
///
/// Holds a reference to the parent <see cref="ExpressionTransformer"/> for recursive
/// expression / statement transformation and indirect access to the
/// <see cref="PatternMatchingHandler"/> for arm patterns.
/// </summary>
public sealed class SwitchHandler(ExpressionTransformer parent, PatternMatchingHandler patterns)
{
    private readonly ExpressionTransformer _parent = parent;
    private readonly PatternMatchingHandler _patterns = patterns;

    public TsSwitchStatement TransformSwitchStatement(SwitchStatementSyntax switchStmt)
    {
        var discriminant = _parent.TransformExpression(switchStmt.Expression);
        var cases = new List<TsSwitchCase>();

        foreach (var section in switchStmt.Sections)
        {
            var body = section.Statements.Select(_parent.TransformStatement).ToList();

            foreach (var label in section.Labels)
            {
                switch (label)
                {
                    case CaseSwitchLabelSyntax caseLabel:
                        cases.Add(new TsSwitchCase(_parent.TransformExpression(caseLabel.Value), body));
                        break;
                    case DefaultSwitchLabelSyntax:
                        cases.Add(new TsSwitchCase(null, body));
                        break;
                    case CasePatternSwitchLabelSyntax patternLabel:
                        // Pattern-based case → convert pattern to condition and use if-like logic
                        // For now, fall through to default
                        cases.Add(new TsSwitchCase(null, body));
                        break;
                }
            }
        }

        return new TsSwitchStatement(discriminant, cases);
    }

    public TsExpression TransformSwitchExpression(SwitchExpressionSyntax switchExpr)
    {
        var governing = _parent.TransformExpression(switchExpr.GoverningExpression);
        var arms = switchExpr.Arms.ToList();

        // Build a ternary chain: cond1 ? val1 : cond2 ? val2 : default
        return BuildTernaryChain(governing, arms, 0);
    }

    private TsExpression BuildTernaryChain(TsExpression governing, List<SwitchExpressionArmSyntax> arms, int index)
    {
        if (index >= arms.Count)
            return new TsIdentifier("undefined");

        var arm = arms[index];
        var value = _parent.TransformExpression(arm.Expression);

        // Discard pattern (_) → this is the default/else
        if (arm.Pattern is DiscardPatternSyntax)
            return value;

        var condition = _patterns.TransformPatternToCondition(governing, arm.Pattern);

        // Add when clause if present
        if (arm.WhenClause is not null)
        {
            var whenExpr = _parent.TransformExpression(arm.WhenClause.Condition);
            condition = new TsBinaryExpression(condition, "&&", whenExpr);
        }

        var rest = BuildTernaryChain(governing, arms, index + 1);
        return new TsConditionalExpression(condition, value, rest);
    }
}
