using SampleCounterV4.Inferno;
using SampleCounterV4.Models;
using static SampleCounterV4.Mvu.Ui;

namespace SampleCounterV4.Components;

/// <summary>
/// Stateful Inferno component. Subclasses Inferno's <c>Component</c>
/// (imported from <c>inferno</c>) and overrides <see cref="Render"/>
/// to project the current state into a virtual-DOM tree built by the
/// JSX-flavored widget facade.
/// </summary>
public sealed class CounterApp : Component<EmptyProps, Counter>
{
    public override InfernoElement Render()
    {
        var state = State ?? Counter.Zero;

        return Column(
            gap: 12,
            Heading($"Count: {state.Count}", level: 1),
            Row(
                gap: 8,
                Button("➖", onClick: () => SetState(state.Decrement())),
                Button("➕", onClick: () => SetState(state.Increment())),
                Button("Reset", onClick: () => SetState(Counter.Zero))
            )
        );
    }
}
