namespace Metano.Annotations;

/// <summary>
/// Selects how an <c>[Inline]</c> member's body lands at each call site.
/// </summary>
public enum InlineMode
{
    /// <summary>
    /// Default. The body materializes as an inline function value at every
    /// site — invocations become <c>((p) => body)(arg)</c> (an IIFE) so each
    /// caller argument evaluates exactly once. References to the member as a
    /// value yield the closure itself.
    /// </summary>
    Materialize = 0,

    /// <summary>
    /// β-reduction: every parameter is substituted directly with the caller's
    /// argument expression inside the body. The lowered call site reads as if
    /// the body were typed in place. Caller arguments may be evaluated more
    /// than once when the body references the parameter multiple times — use
    /// only when the arguments are pure, or when duplication is intentional
    /// (matches the pre-<c>InlineMode</c> behavior).
    /// </summary>
    Substitute = 1,
}

/// <summary>
/// Marks a <c>static readonly</c> field, a <c>static</c> property with an
/// expression-bodied getter, or a <c>static</c> method whose body is a single
/// expression for use-site inlining: every reference (or call) is replaced by
/// the member's body. The declaration itself does not emit a top-level export.
/// <para>
/// <see cref="Mode"/> selects the lowering shape — see <see cref="InlineMode"/>
/// for the trade-off between safe single-evaluation and direct substitution.
/// </para>
/// <para>
/// Applies to:
/// </para>
/// <list type="bullet">
///   <item><c>static readonly</c> fields with an initializer.</item>
///   <item><c>static</c> properties with an expression-bodied getter.</item>
///   <item><c>static</c> methods whose body is an expression body
///   (<c>=&gt; expr</c>) or a single <c>return expr;</c>. Extension members
///   count as static. Named and optional arguments are honored.</item>
/// </list>
/// <para>
/// Invalid shapes (instance fields, mutable fields, instance methods, methods
/// with multi-statement bodies, or properties with block-bodied accessors)
/// raise <c>MS0016 InvalidInline</c>. Recursion through <c>[Inline]</c>
/// members is detected by the frontend validator and surfaces the same code
/// with a cycle message.
/// </para>
/// </summary>
[AttributeUsage(
    AttributeTargets.Field
        | AttributeTargets.Property
        | AttributeTargets.Method
        | AttributeTargets.Class,
    Inherited = false
)]
public sealed class InlineAttribute(InlineMode mode = InlineMode.Materialize) : Attribute
{
    public InlineMode Mode { get; } = mode;
}
