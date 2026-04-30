using Metano.Annotations;
using Metano.Annotations.TypeScript;

namespace SampleCounterV4.Inferno;

/// <summary>Mounts an <see cref="InfernoElement"/> tree into a real DOM container.</summary>
[Erasable]
public static class InfernoRenderer
{
    [Import(name: "render", from: "inferno", Version = "^9.0.0"), Name("render"), External]
    public static void Render(InfernoElement vnode, object container) =>
        throw new NotSupportedException();
}
