# SampleCounterV3

The third counter sample grows the MVU experiment into a component-oriented
shape with stateful widgets, generated component code, style helpers, and a
larger widget set.

## What You Will Find

- `Components/CounterApp.cs` — the C# component entry point.
- `Components/CounterApp.Generated.cs` — generated/supporting component shape
  checked into the sample.
- `Models/Counter.cs` — immutable counter state.
- `Mvu/` — build context, state holder, widget base types, style helpers, and
  UI factories.
- `Mvu/Widgets/` — button, column, heading, row, and text widgets.
- `Program.cs` — mounts `CounterApp`.

This sample is useful for inspecting generated component structures, inherited
UI abstractions, and a deeper namespace tree.

## Generated Code

The related TypeScript source is in
[targets/js/sample-counter-v3/src](../../targets/js/sample-counter-v3/src/):

- `components/counter-app.ts`
- `models/counter.ts`
- `mvu/stateful-widget.ts`
- `mvu/ui.ts`
- `mvu/widgets/*.ts`
- `program.ts`

The built JS package output is in
[targets/js/sample-counter-v3/lib](../../targets/js/sample-counter-v3/lib/).

## Regenerate And Run

```bash
dotnet build samples/SampleCounterV3/SampleCounterV3.csproj
cd targets/js/sample-counter-v3
bun install
bun run build
```

