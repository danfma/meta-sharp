namespace Metano.Annotations;

/// <summary>
/// Marks a <c>static class</c> as a pure compile-time container: the class
/// emits no declaration in the generated output, and every static member
/// access drops the class qualifier (<c>Constants.Pi</c> → <c>Pi</c>).
/// Intended for container types whose members are runtime-provided
/// (<c>[Import]</c>), template-emitted (<c>[Emit]</c>), inline literals
/// (<c>[Inline]</c>), or otherwise already-erased.
/// <para>
/// The attribute's effect is local to the type it decorates. It does not
/// propagate to nested types or to <c>[Inline]</c> members inside the class
/// — member-level inlining policy is controlled exclusively through
/// <see cref="InlineAttribute.Mode"/>.
/// </para>
/// <para>
/// Applies only to <c>static class</c>. Non-static targets and combinations
/// with <c>[Transpile]</c> raise <c>MS0015 InvalidNoContainer</c>.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class NoContainerAttribute : Attribute;
