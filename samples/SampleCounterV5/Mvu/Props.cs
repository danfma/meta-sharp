using Metano.Annotations;

namespace SampleCounterV5.Mvu;

/// <summary>
/// Inferno's standard intrinsic-element prop record. Carries the
/// <c>className</c> attribute that the runtime maps to the DOM
/// <c>class</c> attribute. <c>[PlainObject]</c> emits the C# record
/// as a plain TS object literal — no class wrapper, no constructor —
/// so the call site lowers to <c>{ className: "..." }</c>.
/// </summary>
[PlainObject]
public sealed record DomProps(string? ClassName = null);

/// <summary>Inferno button props — class plus a click handler.</summary>
[PlainObject]
public sealed record ButtonProps(string? ClassName, Action OnClick);
