using SampleCounterV3.Models;
using SampleCounterV3.Mvu;
using static SampleCounterV3.Mvu.Ui;

namespace SampleCounterV3.Components;

public sealed partial class CounterApp : StatefulWidget<Counter>
{
    public override Counter Initial() => Counter.Zero;

    protected override Widget Build(BuildContext<Counter> ctx) =>
        Column(
            gap: 12,
            Heading($"Count: {ctx.State.Count}", level: 1),
            Row(
                gap: 8,
                Button("➖", onPressed: () => ctx.SetState(s => s.Decrement())),
                Button("➕", onPressed: () => ctx.SetState(s => s.Increment())),
                Button("Reset", onPressed: () => ctx.SetState(_ => Counter.Zero))
            )
        );
}
