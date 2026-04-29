using Metano.Annotations;

namespace SampleCounterV5.Bindings;

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
    public static InfernoElement H(
        object tagOrComponent,
        object? props,
        params InfernoElement[] children
    ) => throw new NotSupportedException();
}

/// <summary>Mounts an <see cref="InfernoElement"/> tree into a real DOM container.</summary>
[Erasable]
public static class InfernoRender
{
    [Import(name: "render", from: "inferno", Version = "^9.0.0"), Name("render")]
    public static void Render(InfernoElement vnode, object container) =>
        throw new NotSupportedException();
}
