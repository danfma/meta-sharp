namespace Metano.Annotations;

/// <summary>
/// Marks a <c>static readonly</c> field, a <c>static</c> property
/// with an expression-bodied getter, or a <c>static</c> method whose
/// body is a single expression for use-site inlining: every
/// reference (or call) is replaced by the member's initializer,
/// getter expression, or method body before lowering, with each
/// parameter substituted by the caller's argument. The declaration
/// itself does not emit a top-level <c>export const</c> / function;
/// the value lives exclusively at the call sites.
/// <para>
/// Enables catalog-style APIs whose entries must erase into their
/// literal form at the call site. Combined with <c>[Erasable]</c> on
/// the containing class and <c>[PlainObject]</c> / <c>[Branded]</c>
/// on the initializer's type, a call like
/// <c>HtmlElementType.Div</c> lowers directly to the literal shape
/// the runtime expects, matching the TypeScript overload-on-literal
/// pattern without a helper indirection. For methods (typically
/// extension methods), the body folds into the call site so a typed
/// helper such as <c>document.CreateElement(HtmlElementType.Div)</c>
/// reduces to a single native <c>document.createElement("div")</c>.
/// </para>
/// <para>
/// Applies to:
/// </para>
/// <list type="bullet">
///   <item><c>static readonly</c> fields with an initializer.</item>
///   <item><c>static</c> properties with an expression-bodied getter
///   (<c>public static T Div =&gt; new("div");</c>).</item>
///   <item><c>static</c> methods whose body is either an expression
///   body (<c>=&gt; expr</c>) or a block containing a single
///   <c>return expr;</c>. Extension members count as static. Named
///   and optional arguments are honored: each callee parameter is
///   bound to the caller's matching named argument or to the
///   parameter's explicit default when the caller skipped it.</item>
/// </list>
/// <para>
/// Invalid shapes (instance fields, mutable fields, instance methods,
/// methods with multi-statement bodies, or properties with
/// block-bodied accessors) raise <c>MS0016 InvalidInline</c>.
/// Recursion through <c>[Inline]</c> members (<c>A =&gt; B</c>,
/// <c>B =&gt; A</c>, or <c>A =&gt; A</c>) is detected by the frontend
/// validator via a DFS over each member's initializer graph and
/// surfaces the same code with a cycle message, so a self-referential
/// catalog fails at the validation phase rather than recursing during
/// extraction.
/// </para>
/// <para>
/// <c>[Inline]</c> lives in <see cref="Metano.Annotations"/> because
/// the semantic (use-site substitution) is meaningful for every
/// target; each backend decides how to realize the substitution in
/// its own AST.
/// </para>
/// </summary>
[AttributeUsage(
    AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method,
    Inherited = false
)]
public sealed class InlineAttribute : Attribute;
