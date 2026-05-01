# Cross-Project References

Metano supports sharing types across **multiple C# projects** that each produce
their own npm package. This guide explains how it works and how to set it up.

## The scenario

You have a solution with multiple C# projects:

```
MySolution/
├── MySolution.Shared/          # → npm: @acme/shared
├── MySolution.Users/           # → npm: @acme/users (depends on Shared)
└── MySolution.Orders/          # → npm: @acme/orders (depends on Shared + Users)
```

Each project transpiles to its own npm package. Types in `Orders` can reference
types in `Users` and `Shared`, and the generated TypeScript gets the right
`import` statements and `package.json` entries **automatically**.

## Setup

### Each project declares its npm package identity

In each `.csproj`, add an `AssemblyInfo.cs` (or similar) with:

```csharp
using Metano.Annotations;

[assembly: TranspileAssembly]
[assembly: EmitPackage("@acme/shared", Version = "workspace:*")]
```

- **`[assembly: TranspileAssembly]`** — marks the assembly for whole-project
  transpilation (optional, but usually what you want)
- **`[assembly: EmitPackage(name, Version)]`** — the npm package name and the
  version specifier to use in consumer `package.json#dependencies`

The `Version` field is optional. Common values:

- `"workspace:*"` — for Bun/pnpm/Yarn workspace siblings
- `"^1.2.3"` — for a published npm version
- Omit entirely — Metano falls back to `workspace:*` if the assembly has no
  explicit `Version`, or `^Major.Minor.Patch` from the assembly's
  `Identity.Version` otherwise

### Consumer project references the producer via `ProjectReference`

In `MySolution.Users.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="../MySolution.Shared/MySolution.Shared.csproj" />
</ItemGroup>
```

This is a **normal .NET project reference**. Metano picks up the reference
during compilation, walks the referenced assembly's public types, and registers
them in its cross-assembly type map.

The TypeScript CLI also finds the target package root by walking upward from
`MetanoOutputDir` until it sees a `package.json`. If no package file is found,
it falls back to the legacy convention: the parent directory of the output
folder. Set `MetanoPackageRoot` when your generated files live somewhere unusual.

## What Metano generates

When a consumer uses a type from a referenced project, the transpiler:

1. **Resolves the type origin** — finds the producer assembly's `[EmitPackage]`
   name + the type's namespace path.
2. **Emits an `import` statement** — `import { TypeName } from "@acme/shared/namespace/path"`
3. **Adds a `package.json` dependency** — the consumer's generated `package.json`
   gets `"@acme/shared": "workspace:*"` automatically.
4. **Merges multiple names from the same barrel** — if you import `Money`,
   `Currency`, and `Price` all from `@acme/shared/finance`, they get one combined
   import line.

### Example

**`Shared/Money.cs`:**

```csharp
using Metano.Annotations;

[assembly: TranspileAssembly]
[assembly: EmitPackage("@acme/shared")]

namespace Acme.Shared.Finance;

public record Money(decimal Amount, string Currency);
```

**`Users/User.cs`:**

```csharp
using Acme.Shared.Finance;

namespace Acme.Users;

public record User(string Id, string Name, Money Balance);
```

**Generated `@acme/users/src/user.ts`:**

```typescript
import { HashCode } from "metano-runtime";
import { Money } from "@acme/shared/finance";

export class User {
  constructor(
    readonly id: string,
    readonly name: string,
    readonly balance: Money,
  ) {}
  // …
}
```

**Generated `@acme/users/package.json`:**

```json
{
  "name": "@acme/users",
  "dependencies": {
    "@acme/shared": "workspace:*",
    "metano-runtime": "^0.1.0"
  }
}
```

## Namespace-first import resolution

Metano uses **namespace-first** imports, which means the import path mirrors the
C# namespace, not the file layout:

| Producer namespace | Generated import |
|---|---|
| `Acme.Shared` (= root namespace) | `from "@acme/shared"` |
| `Acme.Shared.Finance` | `from "@acme/shared/finance"` |
| `Acme.Shared.Finance.Currency` | `from "@acme/shared/finance/currency"` |

The import resolves to the **namespace barrel** (`index.ts` inside that
directory), not a specific file. This keeps imports clean and aligned with the
C# mental model.

**Same-namespace imports** are the one exception: they use relative paths
(`./money`) to avoid cycles through the barrel.

## Multiple packages, same type name

If two referenced assemblies both declare a public type called `User`, Metano
disambiguates at the **symbol level** (via Roslyn), not by string matching. Each
`User` ends up correctly imported from its own producing package — no collision.

## Publishing-friendly flow

When you publish your packages to npm:

1. Set `[assembly: EmitPackage(name, Version = "^x.y.z")]` with the concrete
   published version
2. Consumers see `"@acme/shared": "^x.y.z"` in their generated `package.json`
3. The normal npm/yarn/pnpm/bun install flow resolves dependencies

For monorepo development, use `Version = "workspace:*"` (or omit `Version`
entirely to let it fall back automatically) so Bun/pnpm/Yarn workspaces link
the packages symbolically.

## Diagnostics you might hit

### `MS0007` — Cross-package type without `[EmitPackage]`

If a referenced assembly has `[assembly: TranspileAssembly]` but **no**
`[assembly: EmitPackage]`, the transpiler doesn't know what npm package name to
use for imports. You'll get one `MS0007` error per type that's referenced from
that assembly.

**Fix:** add `[assembly: EmitPackage("name")]` to the producer.

### `MS0008` — Conflicting namespaces under the same `[EmitInFile]` name

If two types share `[EmitInFile("foo")]` but live in different C# namespaces,
Metano rejects the setup because consumers can't resolve which file to import
from. Move them into the same namespace or use different file names.

## The `.metalib` future (NuGet packaging)

The current cross-project flow works for **source-available** dependencies (via
`ProjectReference` in the same solution, or source packages). For NuGet packages
that don't include source, Metano needs a metadata sidecar file called
`.metalib` to provide the type signatures without full source access.

**Status:** not yet implemented. Tracked as
[issue #27](https://github.com/danfma/metano/issues/27) — schema, generation,
embedding, and consumption. The design rationale for reusing Roslyn compilation
references as the primary cross-assembly channel (and leaving `.metalib` as an
additive follow-up) is captured in
[ADR-0004](adr/0004-cross-project-references-via-roslyn.md).

For now, share source via `ProjectReference` within the same solution or via a
monorepo.

## See also

- [Attribute Reference](attributes.md) — `[EmitPackage]`, `[EmitInFile]` details
- [Architecture Overview](architecture.md) — how cross-assembly discovery works internally
- [SampleTodo.Service sample](../samples/SampleTodo.Service/README.md) — real cross-project example
