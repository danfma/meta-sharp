# SampleCounterV5

The fifth counter sample targets SolidJS. The domain model and store are written
in C#, while the generated TypeScript package exposes Solid-friendly state and
interop wrappers.

## What You Will Find

- `Models/Counter.cs` — immutable counter state with `Increment` and
  `Decrement`.
- `Stores/CounterStore.cs` — store API that owns a Solid signal and exposes
  domain methods.
- `JsInterop/SolidJs/` — C# facades for `createSignal`, `createEffect`, raw
  Solid signals, and a wrapper that presents them as `ISignal<T>`.

This sample is useful for generic interop wrappers, `[Import]`, `[NoContainer]`,
TypeScript-only `[External]`, `[Emit]` templates, callbacks, and framework
state primitives.

## Generated Code

The related TypeScript source is in
[targets/js/sample-counter-v5/src](../../targets/js/sample-counter-v5/src/):

- `models/counter.ts`
- `stores/counter-store.ts`
- `js-interop/solid-js/*.ts`
- `views/app-view.tsx`
- `main.tsx`

The built JS package output is in
[targets/js/sample-counter-v5/lib](../../targets/js/sample-counter-v5/lib/).

## Regenerate And Run

```bash
dotnet build samples/SampleCounterV5/SampleCounterV5.csproj
cd targets/js/sample-counter-v5
bun install
bun run build
```
