// Hand-written adapter overrides Metano's generated bindings/inferno.ts.
// Methods marked `[Import(name, from)]` on the C# side currently emit
// stub functions that throw `NotSupportedException()` at runtime — the
// emit pipeline doesn't yet rewrite the bodies as re-exports. The
// MSBuild target `Metano.Build.targets` (sample-local extension) copies
// this file over the generated stub after the Metano CLI completes, so
// the consumer's `import { createElement } from "#/bindings/inferno"`
// resolves to the real npm export.
//
// Tracking issue: TODO file follow-up.
export { createElement } from "inferno-create-element";
