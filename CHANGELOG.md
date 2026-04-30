# Changelog

All notable changes to Metano are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project
adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
