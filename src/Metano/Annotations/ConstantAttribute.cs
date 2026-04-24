namespace Metano.Annotations;

/// <summary>
/// Marks a parameter or field whose value must be a compile-time
/// constant literal. Violations surface as <c>MS0014
/// InvalidConstant</c>.
/// <para>
/// <b>Parameter contract.</b> Every argument passed at the call
/// site must resolve to one of:
/// </para>
/// <list type="bullet">
///   <item>A Roslyn <c>ConstantValue</c> expression (literal token,
///   <c>const</c> local, <c>const</c> field).</item>
///   <item>A reference to another <c>[Constant]</c>-decorated field
///   whose own initializer has already been validated.</item>
/// </list>
/// <para>
/// References to ordinary <c>readonly</c> fields are <b>not</b>
/// accepted — Roslyn does not treat them as constant expressions,
/// and the transpiler refuses to chase the initializer without the
/// explicit <c>[Constant]</c> attribute on the source field.
/// </para>
/// <para>
/// <b>Field contract.</b> The decorated field must be <c>const</c>
/// or <c>readonly</c> and carry an initializer that Roslyn reduces
/// to a constant (literal token or <c>const</c> reference). Mutable
/// fields are rejected: if the value can be reassigned after
/// construction, downstream lowering cannot trust it as a compile-time
/// literal.
/// </para>
/// <para>
/// The attribute exists so downstream lowering (<c>[Emit]</c>
/// templates with literal-only substitution, <c>[Inline]</c>
/// expansion that needs the caller's value in source form) can
/// rely on the value being known at compile time without a
/// separate analyzer pass.
/// </para>
/// <para>
/// <c>[Constant]</c> lives in <see cref="Metano.Annotations"/>
/// because the semantic (compile-time literal value) is meaningful
/// for every target; per-target consumers layer on top.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field, Inherited = false)]
public sealed class ConstantAttribute : Attribute;
