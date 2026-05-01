# SampleCounterV1

The first counter sample uses a classic MVP-style shape: a C# model,
presenter, view interface, DOM-backed view, and a top-level bootstrap.

## What You Will Find

- `Models/Counter.cs` — immutable counter domain model.
- `Presenters/CounterPresenter.cs` — presenter logic that coordinates model and
  view.
- `Presenters/ICounterView.cs` — generated as a TypeScript interface.
- `Views/` — a small DOM rendering layer using the TypeScript DOM bindings.
- `Program.cs` — creates the view and presenter, then starts the app.

This sample is useful when you want to inspect classes, interfaces, constructor
injection, DOM facades, and generated namespace folders.

## Generated Code

The related TypeScript source is in
[targets/js/sample-counter-v1/src](../../targets/js/sample-counter-v1/src/):

- `models/counter.ts`
- `presenters/counter-presenter.ts`
- `presenters/i-counter-view.ts`
- `views/counter-view.ts`
- `views/renderer.ts`
- `program.ts`

The built JS package output is in
[targets/js/sample-counter-v1/lib](../../targets/js/sample-counter-v1/lib/).

There is also a Flutter/Dart consumer related to this sample in
[targets/flutter/sample_counter](../../targets/flutter/sample_counter/). Its
generated Dart model/presenter files live in
[targets/flutter/sample_counter/lib/sample_counter](../../targets/flutter/sample_counter/lib/sample_counter/).
That output is produced by the Dart compiler target rather than the MSBuild
TypeScript hook.

## Regenerate And Run

```bash
dotnet build samples/SampleCounterV1/SampleCounterV1.csproj
cd targets/js/sample-counter-v1
bun install
bun run build
```

To regenerate the related Dart output:

```bash
dotnet run --project src/Metano.Compiler.Dart/ -- \
  -p samples/SampleCounterV1/SampleCounterV1.csproj \
  -o targets/flutter/sample_counter/lib/sample_counter \
  --clean
```
