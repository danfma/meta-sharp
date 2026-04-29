using SampleCounterV5.Bindings;
using SampleCounterV5.Components;

// Inferno's `createElement` accepts the component class as the first
// argument. The `Inferno.Of<TComponent>` helper uses the new `$T0`
// type-arg placeholder (#189) to embed `CounterApp` literally in the
// emitted call — no `typeof(...)` expression needed.
InfernoRender.Render(Inferno.Of<CounterApp>(new EmptyProps()), Dom.GetOrCreateRoot("root"));
