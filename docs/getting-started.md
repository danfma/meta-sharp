# Getting Started

This guide walks you through creating your first Metano project from scratch —
annotating a C# class, running the transpiler, and consuming the generated
TypeScript from a Bun project.

## Prerequisites

- **.NET SDK 10.0** (preview) — [download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Bun 1.3+** — [install](https://bun.sh)
- **Git**

## Step 1: Create a C# class library

```bash
mkdir my-domain && cd my-domain
dotnet new classlib
```

Edit the generated `.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <LangVersion>preview</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Metano" Version="0.1.*" />
    <PackageReference Include="Metano.Build" Version="0.1.*" />
  </ItemGroup>

  <PropertyGroup>
    <MetanoOutputDir>../my-domain-ts/src</MetanoOutputDir>
    <MetanoClean>true</MetanoClean>
  </PropertyGroup>
</Project>
```

Two packages:
- **`Metano`** — the attributes (`[Transpile]`, `[StringEnum]`, etc.) and BCL runtime
  mappings
- **`Metano.Build`** — MSBuild integration that runs the transpiler after
  `dotnet build`

## Step 2: Write some C#

Delete the default `Class1.cs` and create `Product.cs`:

```csharp
using Metano.Annotations;

[assembly: TranspileAssembly]
[assembly: EmitPackage("my-domain")]

namespace MyDomain;

[StringEnum]
public enum Category
{
    Books,
    Electronics,
    Clothing,
}

public record Product(string Name, decimal Price, Category Category)
{
    public Product ApplyDiscount(decimal percent) =>
        this with { Price = Price * (1 - percent / 100) };

    public bool IsExpensive => Price > 100;
}
```

What these attributes do:

- **`[assembly: TranspileAssembly]`** — transpile every public type in this
  assembly. Without it, you'd have to mark each type with `[Transpile]` individually.
- **`[assembly: EmitPackage("my-domain")]`** — sets the npm package name for the
  generated TypeScript output. If another C# project references this one, its
  imports will resolve to `import { Product } from "my-domain"`.
- **`[StringEnum]`** — emits `Category` as a string union (`"Books" | "Electronics" | "Clothing"`)
  instead of a numeric enum.

## Step 3: Build

```bash
dotnet build
```

You should see output like:

```
Metano: transpiling MyDomain...
  Generated: my-domain/src/category.ts
  Generated: my-domain/src/product.ts
  Generated: my-domain/src/index.ts
  Updated: my-domain/package.json
Metano: 3 file(s) generated in ../my-domain-ts/src
```

## Step 4: Inspect the output

`../my-domain-ts/src/category.ts`:

```typescript
export const Category = {
  Books: "Books",
  Electronics: "Electronics",
  Clothing: "Clothing",
} as const;

export type Category = (typeof Category)[keyof typeof Category];
```

`../my-domain-ts/src/product.ts`:

```typescript
import { HashCode } from "metano-runtime";
import { Decimal } from "decimal.js";
import type { Category } from "./category";

export class Product {
  constructor(
    readonly name: string,
    readonly price: Decimal,
    readonly category: Category,
  ) {}

  applyDiscount(percent: Decimal): Product {
    return this.with({
      price: this.price.times(new Decimal(1).minus(percent.dividedBy(100))),
    });
  }

  get isExpensive(): boolean {
    return this.price.greaterThan(100);
  }

  equals(other: any): boolean {
    return (
      other instanceof Product &&
      this.name === other.name &&
      this.price.equals(other.price) &&
      this.category === other.category
    );
  }

  hashCode(): number {
    const hc = new HashCode();
    hc.add(this.name);
    hc.add(this.price);
    hc.add(this.category);
    return hc.toHashCode();
  }

  with(overrides?: Partial<Product>): Product {
    return new Product(
      overrides?.name ?? this.name,
      overrides?.price ?? this.price,
      overrides?.category ?? this.category,
    );
  }
}
```

The transpiler also generated `package.json` with:

```json
{
  "name": "my-domain",
  "type": "module",
  "dependencies": {
    "metano-runtime": "^0.1.0",
    "decimal.js": "^10.6.0"
  }
}
```

## Step 5: Consume from a Bun project

```bash
cd ../my-domain-ts
bun install
```

Create a small script:

```typescript
// test.ts
import { Product, Category } from "./src";
import { Decimal } from "decimal.js";

const book = new Product("Clean Code", new Decimal(45), "Books");
console.log(book.isExpensive);       // false
console.log(book.applyDiscount(new Decimal(10)).price.toString()); // "40.5"
```

Run it:

```bash
bun run test.ts
```

## Where to go next

- **[Attribute Reference](attributes.md)** — Learn every attribute Metano supports
- **[BCL Type Mappings](bcl-mappings.md)** — See what C# types become in TypeScript
- **[Cross-Project References](cross-package.md)** — Share types between multiple C# projects
- **[JSON Serialization](serialization.md)** — Transpile `JsonSerializerContext` for JSON round-trips
- **[Sample projects](../samples/)** — See realistic examples:
  - [SampleTodo](../samples/SampleTodo/README.md) — basic records + enums
  - [SampleTodo.Service](../samples/SampleTodo.Service/README.md) — Hono CRUD
  - [SampleIssueTracker](../samples/SampleIssueTracker/README.md) — complex domain
