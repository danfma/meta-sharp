---
name: IR captures ctor-param-to-field through CapturedFieldName
description: How the IR models DI-style primary-ctor parameter capture into a backing field; the contract bridges already implement.
type: project
---

The IR already carries the contract for "primary-ctor parameter captured into a backing field":

- `IrConstructorParameter.CapturedFieldName` (string?, in `IR/IrConstructorDeclaration.cs`) names the field that holds the captured param.
- `IrFieldDeclaration.IsCapturedByCtor` (bool, in `IR/IrMemberDeclaration.cs`) signals to bridges that the field's `Initializer` must be suppressed (the ctor body assigns it instead).
- `IrClassExtractor.AnnotateCapturedParams` (post-pass after member + ctor extraction) is the single producer of these flags. Today it only handles **explicit** captures (`private readonly Foo _foo = foo;`) by matching field initializers that are bare `IrIdentifier`s pointing at a ctor parameter.
- `IrToTsClassBridge.BuildCapturedCtorParams` and `BuildSimpleConstructor` already consume the contract on the TS side: captured params render with `TsAccessibility.None` (no shorthand-property promotion) and the ctor body emits `this._foo = foo;` per captured pair.

**Why:** When extending capture detection (e.g. issue #141 — implicit captures via method-body references), do not invent new IR fields. Extend `AnnotateCapturedParams` to also synthesize fields for params that are referenced from non-ctor member bodies, and add a body-rewrite branch in `IrExpressionExtractor.ExtractIdentifierName` that turns a captured `IParameterSymbol` into `IrMemberAccess(this, fieldName)`. Bridges and Dart parity follow for free.

**How to apply:** Before designing capture-related changes, check whether `CapturedFieldName`/`IsCapturedByCtor` already covers the shape. The bridge contract is the source of truth — match it, don't fork it.
