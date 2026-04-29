using Metano.TypeScript.DOM;
using SampleCounterV5.Components;
using SampleCounterV5.Inferno;

// Inferno's `createElement` accepts the component class as the first
// argument. The `Inferno.Of<TComponent>` helper uses the new `$T0`
// type-arg placeholder (#189) to embed `CounterApp` literally in the
// emitted call — no `typeof(...)` expression needed.
InfernoRenderer.Render(
    Inferno.Of<CounterApp>(new EmptyProps()),
    Js.Document.GetOrCreateElementById("root")
);
