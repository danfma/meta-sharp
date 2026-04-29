namespace Metano.Annotations;

/// <summary>
/// Marks a method or class so the transpiler lowers its parameter
/// list as a single object literal at the TypeScript surface — the
/// "props as object" idiom JSX/Solid/React APIs depend on. Each
/// parameter becomes a field on the synthesized object type;
/// optional parameters (those with a default value) render with the
/// <c>?:</c> suffix, required parameters render as plain
/// <c>p: T</c>. The TS body destructures the argument back into
/// the original parameter names so existing references keep
/// working without touching the original C# body.
/// </summary>
/// <example>
/// <code>
/// [ObjectArgs]
/// public static Column Column(int gap = 0, Widget[] children) => new(gap, children);
///
/// // Call sites — positional, named, or mixed — collapse to an object literal:
/// UI.Column(gap: 12, children: x);  // → UI.column({ gap: 12, children: x })
/// UI.Column(children: x);           // → UI.column({ children: x })
/// </code>
/// </example>
[AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class,
    Inherited = false
)]
public sealed class ObjectArgsAttribute : Attribute;
