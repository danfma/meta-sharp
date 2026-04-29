using Metano.Annotations;

namespace SampleCounterV5.Bindings;

/// <summary>
/// Tiny DOM facade. <c>[Erasable]</c> drops the static qualifier at the
/// call site so <c>Dom.GetOrCreateRoot("root")</c> emits as
/// <c>getOrCreateRoot("root")</c> when imported, but the
/// <c>[Emit(...)]</c> template inlines the body verbatim — no helper
/// function is exported.
/// </summary>
[Erasable]
public static class Dom
{
    [Emit(
        "(document.getElementById($0) ?? (() => { const el = document.createElement(\"div\"); el.id = $0; document.body.append(el); return el; })())"
    )]
    public static object GetOrCreateRoot(string id) => throw new NotSupportedException();
}
