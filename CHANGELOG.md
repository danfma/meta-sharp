# Changelog

All notable changes to Metano are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project
adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 2.1.0

_2026-05-02_


### ✨ Features

* **cli:** add --dry-run flag ([#19](https://github.com/danfma/metano/issues/19)) ([d035bba](https://github.com/danfma/metano/commit/d035bba173076abb3c8b6fd26f5db80419542207))
* **ts:** support direct recursion in [Inline] methods via named function expression ([#194](https://github.com/danfma/metano/issues/194)) ([04f5813](https://github.com/danfma/metano/commit/04f5813c3ba1dcfd79e1978980399fcb69cab9c1))

### 🐛 Bug Fixes

* **compiler:** normalize args at [Emit] template call site ([#192](https://github.com/danfma/metano/issues/192)) ([221f54d](https://github.com/danfma/metano/commit/221f54dac25da209f79ccc61ac6277cce036e110))
* **compiler:** propagate [Name(target, ...)] through method/interface type references ([#170](https://github.com/danfma/metano/issues/170)) ([4cc97be](https://github.com/danfma/metano/commit/4cc97be9bd614843f3e0d78ee18529ec1a10afe4))
* **import-collector:** register [NoContainer] method-group references for import ([#179](https://github.com/danfma/metano/issues/179)) ([b88595e](https://github.com/danfma/metano/commit/b88595e81e3e2930fe6728329389a21b5417551c))
* **import-collector:** scan referenced assemblies for [NoContainer] exports ([#178](https://github.com/danfma/metano/issues/178)) ([f171da5](https://github.com/danfma/metano/commit/f171da59a9f40ff1ae5696df65fdccf5d6582ebf))
* **ts:** drop parameter-property modifiers on dispatcher overload sigs + rest impl ([#25](https://github.com/danfma/metano/issues/25)) ([d5e2f6d](https://github.com/danfma/metano/commit/d5e2f6dfbabca88324ac7fb86650123b854aa03b))
* **ts:** split record positional params into field + rest ctor parameter ([#152](https://github.com/danfma/metano/issues/152)) ([dbf3fc3](https://github.com/danfma/metano/commit/dbf3fc3bbc5ecb1c5b601e2a542102a09a5f74e4)), closes [#145](https://github.com/danfma/metano/issues/145)

### ♻️ Refactor

* **import-collector:** bundle walker buckets into ImportCollectionSink ([#192](https://github.com/danfma/metano/issues/192)) ([9a942d2](https://github.com/danfma/metano/commit/9a942d2dfc93bdcf5bab9a761439c6b2a6471fe7))

### 📝 Documentation

* **annotations:** pin [External] + [Import] resolution recipe ([#190](https://github.com/danfma/metano/issues/190)) ([285ceb2](https://github.com/danfma/metano/commit/285ceb25d955de47261909e89c6ed4dc65770bff))

## 2.0.0

_2026-05-01_


### ⚠ BREAKING CHANGES

* **annotations:** [NoEmit] and [NoTranspile] are removed. Replace any
[NoEmit] usage with [Ignore] (semantics carry over). Replace any
[NoTranspile] with [Ignore] when the type is .NET-only, or with [External]
when it is an ambient TypeScript shape that transpiled code legitimately
references. Member-level [NoTranspile] without a replacement should switch
to member-level [Ignore].

Compiler changes:

- ValidateIgnoreReferences now skips members marked [Ignore] (an [Ignore]
  helper that takes an [Ignore] marker stays silent).
- Member-level reference validation: invocations and member access on
  [Ignore] methods/properties/fields/events from transpilable code raise
  MS0013 with the member's owner.member name in the message.
- IrTypeRefMapper threads the active TargetLanguage through HasIgnore so a
  type ignored only for the active backend is correctly marked
  IsIgnored=true in the IR (and IsTranspilable=false), keeping downstream
  paths like lambda parameter lowering and runtime guard generation
  consistent with the per-target contract.
- IR field IrNamedTypeSemantics.IsNoEmit renamed to IsIgnored.
- DiagnosticCodes.NoEmitReferencedByTranspiledCode renamed to
  IgnoreReferencedByTranspiledCode (the MS0013 code itself is unchanged).

Tests:

- New IgnoreDotNetOnlyTests pin the MS0013 contract on every reference
  position (parameter, return, field type, body, generic argument,
  per-target gating).
- Former NoEmitTranspileTests renamed to ExternalAmbientTranspileTests; the
  ambient cases all reference [External] now, matching the new semantics.
- Test methods disambiguated (Ignore_ExcludesTypeFromAssemblyWideTranspile,
  Ignore_OverridesExplicitTranspileAttribute, IgnorePerTarget_*).

### ✨ Features

* **annotations:** collapse [NoEmit]/[NoTranspile]/[Ignore] into single [Ignore] ([06b5e43](https://github.com/danfma/metano/commit/06b5e438abe878819f5e5f0d85ba37a9c9d97f04))

### 🐛 Bug Fixes

* **ts:** align import-type form with Biome's expectation ([5714185](https://github.com/danfma/metano/commit/5714185cfc2bca810e2db72552ca2da322e40b08))

## 1.1.0

_2026-05-01_


### ✨ Features

* **compiler:** enhance type checking, import handling, and constructor extraction ([0ec6081](https://github.com/danfma/metano/commit/0ec60812ffbe5dfbe33dd95f78864cd6347651be))

## 1.0.3

_2026-04-30_


### 🐛 Bug Fixes

* **release:** use RELEASE_TAG instead of GITHUB_REF_NAME for npm publish ([13db217](https://github.com/danfma/metano/commit/13db2176009b9c4c115cd0a09f1eac8b3149db5b))

## 1.0.2

_2026-04-30_


### ♻️ Refactor

* **release:** drop dotnet-releaser, use dotnet pack + nuget push directly ([f6c98af](https://github.com/danfma/metano/commit/f6c98af567533b072d63ce5e1747c83a0b250f8f))

## 1.0.1

_2026-04-30_


### 🐛 Bug Fixes

* **release:** install npm deps before metano-runtime build in publish-on-tag ([feba5dc](https://github.com/danfma/metano/commit/feba5dc15fa2302d2e3c8a95783284706f6f27cb))
* **release:** split publish-on-tag into independent NuGet and npm jobs ([7b1a5d2](https://github.com/danfma/metano/commit/7b1a5d23c1468336dc6b45c67a9fd1e240e1d85b))

## 1.0.0

_2026-04-30_


### ✨ Features

* **compiler:** add --file-prefix CLI flag for opaque generated-file headers ([8c05ec1](https://github.com/danfma/metano/commit/8c05ec19d63ad0fb82ea4aae92686ce7357531ee))
* **ir:** materialize [Inline] method as lambda when passed as value ([129661d](https://github.com/danfma/metano/commit/129661de3ca3ece36eb43bce807be0369eb638a9)), closes [#193](https://github.com/danfma/metano/issues/193)

### ♻️ Refactor

* **annotations:** rename [Erasable] → [NoContainer] + add InlineMode ([358fb37](https://github.com/danfma/metano/commit/358fb37985ac827e49eba757b9efda618227f1ed))

### 📝 Documentation

* add ADR-0017 + update spec catalogs for [NoContainer] / InlineMode ([31859c6](https://github.com/danfma/metano/commit/31859c6d7acf7187f588b3d0918c7410992ca7ed))

## 0.9.0

### ✨ Features

- **Dart target prototype.** New `Metano.Compiler.Dart` project ships
  the shape-only Dart backend: `metano_runtime` Dart package with
  hashing/equality primitives, `MetanoObject` base injection,
  `DartImportCollector` wired to runtime requirements, declarative
  BCL mappings via target-aware `[MapMethod]` attributes, and Dart
  delegate lowering to typedefs.
- **`[ObjectArgs]` family.** Object-literal call shape now covers
  static methods, instance methods, and constructors via a `create`
  factory pattern (#163, #167). Type arguments survive the lowering
  (#169) and trailing `params` arguments fold into an array literal
  (#186).
- **`[Emit]` template `$T0` placeholder.** Generic type arguments
  splice into the lowered template verbatim — `Foo.Of<Bar>(...)`
  emits with `Bar` in the template body. Closes #189.
- **Method-level `[Import]` lowering.** A method annotated with
  `[Import(name, from)]` now lowers every call site to a direct
  invocation of the imported identifier and auto-emits the import
  line on the consumer file. The declaring class no longer emits a
  stub. Pairs with `[Emit]` for templated facades. Closes #188.
- **`[ImportAlias]` attribute.** File-scoped TS module carrier for
  picking the import binding name when the C# type's name collides
  (#184). Complements automatic alias synthesis when an erasable
  factory shadows a transpilable type (#183) and propagation of C#
  `using X = Y;` aliases through to TS imports (#182).
- **TypeScript class inheritance.** `extends` + `abstract` modifiers
  emit on the class surface (#118). Sealed hierarchies emit a union
  guard built around a shared discriminator (#88).
- **Extension helper lowering.** Transpilable extension members now
  lower to helper calls at the call site (#156); names propagate
  through `[Name]` and clashes between extension classes raise the
  new MS0021 diagnostic.
- **Function shape coverage.** Default parameter values emit on
  methods and functions (#115). Named arguments reorder into
  declaration order (#157). C# `params` map to TS rest parameters
  (#145). `[Transpile]` delegates emit named type aliases (#122).
- **Internal-visibility surface.** `internal` members now reach the
  TS class output instead of being silently dropped (#162).
- **`[Inline]` propagation.** A static class marked `[Inline]`
  propagates the marker to every member, removing the per-member
  bookkeeping (#107).
- **`[Erasable]` diagnostic — MS0020.** Two `[Erasable]` factories
  resolving to the same emitted name surface as a hard error
  pointing at both definitions.
- **`[NoEmit]` / `[External]` redefinition.** `[NoEmit]` becomes a
  pure .NET-only painting marker; `[External]` widens to cover every
  ambient binding shape that previously leaned on `[NoEmit]`. Class-
  level flatten dropped from `[External]` — flatten now requires
  `[Erasable]` opt-in (#106 PR1–5). MS0013 surfaces misuse.
- **DOM bindings library.** New `Metano.TypeScript.DOM` project
  exposes `Document`, `Window`, `HtmlElement`, etc. as `[External]`
  ambient classes; `Js.Document` / `Js.Window` provide `[Erasable]`
  globals shortcuts.
- **SampleCounterV3 + V4 + V5.** V3 reworked as a mini-MVU/Flutter
  DSL. V4 wires a Flutter-style widget facade through `[Erasable]`
  + `[ObjectArgs]`. V5 ships an Inferno virtual-DOM consumer end-to-
  end with a JSX-flavored widget DSL.

### 🐛 Bug Fixes

- C# 14 `extension(R r) { … }` members now emit once instead of twice
  (Roslyn surfaces them both lifted on the static class and inside
  a synthetic empty-name nested type).
- IR fixes: throw expressions lower to an IIFE with a throw statement
  (#160); `new T()` on a generic type parameter raises MS0019 (#161);
  primary-constructor parameter rewrites cover switch/argument/local-
  var positions (#158); override methods are segregated from sibling
  overload groups (#159); abstract method parameter initializers are
  dropped (#147); abstract modifier is suppressed on records (#144);
  default initializers are dropped when a constructor parameter
  default already covers the field (#164, #165).
- TS imports collected through function, tuple, and type-predicate
  types (#148).

### 🧰 Maintenance

- Sample regeneration is now part of CI: `dotnet build` followed by
  `bunx biome format --write targets/` produces the canonical sample
  output and the drift check diffs against it.
- `Metano-packages.slnx` shipped as a solution alias scoped to the
  publishable projects so the release pipeline avoids file-lock
  conflicts with sample `AfterBuild` targets.

## 0.8.1

### 🐛 Bug Fixes

- `package.json` writes are now additive: hand-curated `type`,
  `sideEffects`, `name`, `imports`, and `exports` survive every
  regeneration. The transpiler only seeds missing fields and refreshes
  its own `{types, import}` exports, leaving user-added subpaths and
  augmented conditional fields untouched (#136).
- `metano-runtime` now declares `"sideEffects": false` so consumers'
  bundlers can tree-shake unused helpers. Verified bundle drop from
  175.1 KB to 1.96 KB on a `HashCode`-only entry — 88× smaller (#137).

### 🧰 Maintenance

- `dotnet-releaser.toml` adds Conventional Commits autolabelers so
  `feat:` / `fix:` / `chore:` etc. land in the right release-note
  sections instead of bundling under "🧰 Misc" (#135).
- Release workflow grants `pull-requests: write` and `issues: write`
  so dotnet-releaser can read merged-PR data when assembling the
  changelog.
- Pinned `dotnet-releaser` 0.16.0 → 0.18.1 for the Tomlyn config
  deserialization fix.
- Switched to a static `CHANGELOG.md` driving release notes —
  works around the GitHub `/commits/{sha}/pulls` 5xx flakiness that
  blocked the v0.8.1 changelog auto-generation.

## 0.8.0

See [the v0.8.0 release notes](https://github.com/danfma/metano/releases/tag/v0.8.0)
for the prior changeset.
