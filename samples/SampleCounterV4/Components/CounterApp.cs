using SampleCounterV4.Models;
using SampleCounterV4.Mvu;
using static SampleCounterV4.Mvu.Ui;

namespace SampleCounterV4.Components;

public sealed partial class CounterApp : StatefulWidget<Counter>
{
    public override Counter Initial() => Counter.Zero;

    protected override Widget Build(BuildContext<Counter> ctx) =>
        Column(
            gap: 12,
            Heading($"Count: {ctx.State.Count}", level: 1),
            Row(
                gap: 8,
                children:
                [
                    Button("➖", onPressed: () => ctx.SetState(s => s.Decrement())),
                    Button("➕", onPressed: () => ctx.SetState(s => s.Increment())),
                    Button("Reset", onPressed: () => ctx.SetState(_ => Counter.Zero)),
                ]
            )
        );
}
