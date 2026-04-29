using Metano.Annotations;
using Metano.Annotations.TypeScript;

namespace Metano.TypeScript.DOM;

// `[Erasable]` flattens `Js.Document` → `document` at the call site so
// the emitted TS reaches the runtime globals directly. `[External]` on
// each member keeps the ambient stub contract: no declaration is
// emitted for the property since the runtime itself owns the binding.
[Erasable]
public static class Js
{
    [External, Name("window")]
    public static Window Window => throw new NotSupportedException();

    [External, Name("document")]
    public static Document Document => throw new NotSupportedException();
}
