namespace Metano.Annotations;

/// <summary>
/// Marks a type as <em>.NET-only</em>: it exists in the C# type system for
/// internal logic, source-generator scaffolding, or build-time tooling, but
/// must never cross into transpiled code. Per #106 the transpiler emits
/// <c>MS0013 NoEmitReferencedByTranspiledCode</c> at every reference from a
/// transpilable type's surface area (signature or body) so the boundary
/// stays explicit.
///
/// <code>
/// [NoEmit]
/// public sealed class BuildToolingMarker { }
///
/// // .NET-side helper code freely consumes BuildToolingMarker.
/// public static class InternalBookkeeping
/// {
///     public static List&lt;BuildToolingMarker&gt; Cache { get; } = new();
/// }
///
/// // A transpilable type referencing it raises MS0013.
/// [Transpile]
/// public class Bad
/// {
///     public BuildToolingMarker Field; // ❌ MS0013
/// }
/// </code>
///
/// For ambient TypeScript shapes that <em>do</em> need to be referenced
/// from transpiled code (DOM types, Hono structural shapes, Inferno
/// <c>VNode</c>), use <see cref="TypeScript.ExternalAttribute"/>: it
/// expresses "no emission" without painting transpilable code as broken.
///
/// Contrast with <see cref="NoTranspileAttribute"/>: <c>[NoTranspile]</c>
/// excludes the type from discovery entirely (the compiler pretends it
/// doesn't exist), while <c>[NoEmit]</c> keeps the type discoverable for
/// .NET-side code while still raising MS0013 if transpiled code reaches it.
/// <para>
/// Follows the per-target resolution pattern of <see cref="NameAttribute"/>:
/// <c>[NoEmit(TargetLanguage.Dart)]</c> paints the type as .NET-only on Dart
/// while letting it emit a TS file, and the parameterless form applies to every
/// target.
/// </para>
/// </summary>
[AttributeUsage(
    AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Enum
        | AttributeTargets.Interface,
    AllowMultiple = true
)]
public sealed class NoEmitAttribute : Attribute
{
    public NoEmitAttribute()
    {
        Target = null;
    }

    public NoEmitAttribute(TargetLanguage target)
    {
        Target = target;
    }

    /// <summary>The target this no-emit applies to, or <c>null</c> for the
    /// untargeted (global) form.</summary>
    public TargetLanguage? Target { get; }
}
