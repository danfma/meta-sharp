using Metano.Annotations;

namespace SampleCounterV5.Bindings;

/// <summary>
/// Inferno's virtual-DOM node type. The npm package exports it as
/// <c>VNode</c>; the <c>[Import]</c> attribute aliases the export so
/// consumer files reference the local C# name <c>InfernoElement</c>
/// while the import line emits <c>{ VNode as InfernoElement }</c>.
/// </summary>
[Import(name: "VNode", from: "inferno", Version = "^9.0.0")]
public abstract class InfernoElement;
