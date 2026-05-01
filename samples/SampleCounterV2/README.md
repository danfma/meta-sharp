# SampleCounterV2

The second counter sample experiments with a tiny MVU widget model written in
C#. It keeps the counter state immutable while rendering the UI from a C#
function that returns widget objects.

## What You Will Find

- `Models/Counter.cs` — immutable state with `Increment`.
- `Mvu/App.cs` — mount helper that owns state updates.
- `Mvu/StateHolder.cs` — mutable holder used by the generated runtime shape.
- `Mvu/Widgets/` — simple `Button`, `Column`, and `Text` widgets.
- `Program.cs` — calls `App.Mount` with an initial state and view lambda.

This sample is useful for generated lambdas, callbacks, arrays, simple UI
composition, and nested namespaces.

## Generated Code

The related TypeScript source is in
[targets/js/sample-counter-v2/src](../../targets/js/sample-counter-v2/src/):

- `models/counter.ts`
- `mvu/app.ts`
- `mvu/state-holder.ts`
- `mvu/widgets/button.ts`
- `mvu/widgets/column.ts`
- `mvu/widgets/text.ts`
- `program.ts`

The built JS package output is in
[targets/js/sample-counter-v2/lib](../../targets/js/sample-counter-v2/lib/).

## Regenerate And Run

```bash
dotnet build samples/SampleCounterV2/SampleCounterV2.csproj
cd targets/js/sample-counter-v2
bun install
bun run build
```

