namespace MetaSharp.Annotations;

/// <summary>
/// Declares the target-package identity of the current C# assembly when transpiled.
/// Read by the MetaSharp compiler to (a) write the <c>name</c> field of the generated
/// <c>package.json</c> as an authoritative source, and (b) resolve cross-assembly type
/// references from a consumer project to <c>import { … } from "&lt;package&gt;/…"</c>
/// statements.
///
/// <para>
/// Multiple instances are allowed, one per <see cref="EmitTarget"/> — the C# project can
/// thus be transpiled to several languages, each with its own ecosystem-correct package
/// name:
/// </para>
///
/// <code>
/// [assembly: EmitPackage("sample-todo")]                                  // JavaScript (default)
/// [assembly: EmitPackage("sample_todo", EmitTarget.JavaScript)]           // explicit form
/// </code>
///
/// <para>
/// The attribute is <em>optional</em>. A library project that wants to be referenced
/// by other transpiled projects MUST declare it; an application/program project that
/// only consumes other packages doesn't need it. When a consumer references a type from
/// an assembly that lacks the attribute for the active target, the compiler emits
/// MS0007 at the consumer site.
/// </para>
/// </summary>
/// <param name="name">The package name in the target ecosystem (e.g., <c>"sample-todo"</c>
/// or <c>"@scope/name"</c> for npm).</param>
/// <param name="target">Which compiler target this declaration applies to.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class EmitPackageAttribute(string name, EmitTarget target = EmitTarget.JavaScript) : Attribute
{
    public string Name { get; } = name;
    public EmitTarget Target { get; } = target;
}
