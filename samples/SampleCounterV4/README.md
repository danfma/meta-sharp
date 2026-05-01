# SampleCounterV4

The fourth counter sample focuses on framework interop. It models a small
Inferno binding layer in C#, then emits TypeScript that calls real Inferno APIs.

## What You Will Find

- `Components/CounterApp.cs` — counter component implemented in C#.
- `Inferno/` — C# facades and helpers for Inferno rendering and DOM access.
- `Mvu/Props.cs` and `Mvu/Ui.cs` — props objects and UI factory methods.
- `Models/Counter.cs` — immutable domain state.
- `Program.cs` — renders `CounterApp` through the Inferno facade.

This sample is useful for `[Emit]`, external JS interop, type-argument
placeholders, props objects, and generated imports for a UI framework.

## Generated Code

The related TypeScript source is in
[targets/js/sample-counter-v4/src](../../targets/js/sample-counter-v4/src/):

- `components/counter-app.ts`
- `inferno/*.ts`
- `models/counter.ts`
- `mvu/*.ts`
- `program.ts`

The built JS package output is in
[targets/js/sample-counter-v4/lib](../../targets/js/sample-counter-v4/lib/).

## Regenerate And Run

```bash
dotnet build samples/SampleCounterV4/SampleCounterV4.csproj
cd targets/js/sample-counter-v4
bun install
bun run build
```

