# sample_counter (Flutter)

Flutter consumer of the SampleCounter C# project. The `Counter`, `ICounterView`
and `CounterPresenter` classes in `lib/sample_counter/` are regenerated from
the shared C# source by the Metano Dart target.

## Regenerate the Dart sources

From the repo root:

```sh
dotnet run --project src/Metano.Compiler.Dart/ -- \
  -p samples/SampleCounter/SampleCounter.csproj \
  -o targets/flutter/sample_counter/lib/sample_counter \
  --clean
```

## Run the app

```sh
cd targets/flutter/sample_counter
flutter pub get
flutter run
```

## Status

This is a **prototype** for the Dart/Flutter target — the first second-target
exercise of the Metano IR architecture. Declarations and the covered body
subset now flow end-to-end through the IR: the generated `Counter` ships
with a working `copyWith`, `==`/`hashCode` derived from the record shape,
and no `UnimplementedError` stubs. `lib/main.dart` is a plain Flutter
consumer — no extension-based workarounds — wiring the generated
`CounterPresenter` up to a `MaterialApp`.

Follow-ups tracked in `docs/compiler-refactor-plan.md`: classic extension
methods, the `[ModuleEntryPoint]` body path, and a BCL mapping for
`Console.WriteLine` → `print` so the counter's `Program.cs` entry point
can emit on Dart too (today it's opted out via `[NoEmit(Dart)]`).
