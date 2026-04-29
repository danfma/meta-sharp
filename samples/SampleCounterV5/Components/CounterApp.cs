using SampleCounterV5.Bindings;
using SampleCounterV5.Models;
using static SampleCounterV5.Mvu.Ui;

namespace SampleCounterV5.Components;

/// <summary>
/// Stateful Inferno component. Subclasses Inferno's <c>Component</c>
/// (imported from <c>inferno</c>) and overrides <see cref="Render"/>
/// to project the current state into a virtual-DOM tree built by the
/// JSX-flavored widget facade.
/// </summary>
public sealed class CounterApp : Component<EmptyProps, Counter>
{
    public override InfernoElement Render() =>
        Column(
            gap: 12,
            Heading($"Count: {State!.Count}", level: 1),
            Row(
                gap: 8,
                Button("➖", onClick: () => SetState(State!.Decrement())),
                Button("➕", onClick: () => SetState(State!.Increment())),
                Button("Reset", onClick: () => SetState(Counter.Zero))
            )
        );
}
