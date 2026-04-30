using Metano.Annotations;

namespace SampleCounterV4.Inferno;

/// <summary>
/// Thin facade over <c>inferno-create-element</c>. <c>[Erasable]</c>
/// flattens the static qualifier (<c>Inferno.H(args)</c> →
/// <c>createElement(args)</c>); <c>[Import]</c> + <c>[Name]</c> resolve
/// the emitted call to Inferno's real export.
/// <para>
/// One method covers both the DOM-element shape (<c>tag: string</c>)
/// and the component shape (<c>tag: Type</c>) because Inferno's runtime
/// dispatches on the first argument's type. Children are optional via
/// <c>params</c> so component creation passes only props.
/// </para>
/// </summary>
[Erasable]
public static class Inferno
{
    [
        Import(name: "createElement", from: "inferno-create-element", Version = "^9.0.0"),
        Name("createElement")
    ]
    public static InfernoElement H(string tag, object? props, params InfernoElement[] children) =>
        throw new NotSupportedException();

    /// <summary>
    /// Component-node creation. The <c>$T0</c> placeholder embeds the
    /// emitted name of the type argument — <c>Inferno.Of&lt;CounterApp&gt;(props)</c>
    /// lowers to <c>createElement(CounterApp, props)</c>. The C# generic
    /// constraint is intentionally absent: any class emits cleanly as a
    /// runtime-class reference on the TS side.
    /// </summary>
    [
        Emit("createElement($T0, $0)"),
        Import(name: "createElement", from: "inferno-create-element", Version = "^9.0.0")
    ]
    public static extern InfernoElement Of<TComponent>(object? props);
}
