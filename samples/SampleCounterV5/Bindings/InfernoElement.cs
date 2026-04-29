using Metano.Annotations.TypeScript;

namespace SampleCounterV5.Bindings;

/// <summary>
/// Opaque virtual-DOM node returned by Inferno's <c>createElement</c>.
/// Ambient marker only — no TS file is emitted; consumer code obtains
/// values of this type by calling <see cref="Inferno.H"/> or a widget
/// factory and feeds them back to Inferno's <c>render</c>.
/// </summary>
[External]
public abstract class InfernoElement;
