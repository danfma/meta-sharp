# Attribute Reference

Every Metano attribute lives in the `Metano.Annotations` namespace. Import it
once per file:

```csharp
using Metano.Annotations;
```

This page is the complete reference — every attribute, what it does, and when to
use it.

## Type selection

### `[Transpile]`

Marks a single type for transpilation. Without this (and without
`[assembly: TranspileAssembly]`), a type is **ignored** even if it's public.

```csharp
[Transpile]
public record User(string Name, string Email);
```

### `[assembly: TranspileAssembly]`

Assembly-level attribute. Transpiles **every public type** in the assembly by
default. Individual types can opt out with `[NoTranspile]`.

```csharp
[assembly: TranspileAssembly]
```

Use this when most of your types should cross to TypeScript — less boilerplate
than marking each type individually.

### `[NoTranspile]`

Explicitly exclude a type from transpilation. Only meaningful when the assembly
has `[TranspileAssembly]`.

```csharp
[NoTranspile]
public class InternalHelper { /* … */ }
```

### `[NoEmit]`

Similar to `[NoTranspile]`, but the type is still **discoverable** for reference
resolution. Use this when a type exists in C# only as a declaration target for
cross-package imports and shouldn't produce a `.ts` file.

```csharp
[Transpile, NoEmit]
[Import(name: "Widget", from: "widget-lib")]
public class Widget { /* declaration-only facade */ }
```

## Naming

### `[Name("x")]`

Overrides the name emitted in the TypeScript output. Works on types, methods,
properties, fields, and enum members.

```csharp
[Name("USD")] USDollar,

[Name("api")]
public void CallApi() { /* … */ }

[Name("snake_case_name")]
public string OriginalCamelCase { get; set; }
```

### `[Ignore]`

Omits a member from the TypeScript output entirely.

```csharp
public class User
{
    public string Name { get; set; }

    [Ignore]
    public string InternalDebugInfo { get; set; } // not emitted
}
```

## Output shaping

### `[StringEnum]`

Emits the enum as a **string union** (via a `const` object + `type` alias)
instead of a numeric TypeScript `enum`. Recommended for most enums — more
idiomatic in TypeScript and plays nicely with JSON.

```csharp
[StringEnum]
public enum Priority
{
    Low,
    [Name("medium")] Medium,
    High,
}
```

Becomes:

```typescript
export const Priority = {
  Low: "Low",
  Medium: "medium",
  High: "High",
} as const;
export type Priority = (typeof Priority)[keyof typeof Priority];
```

### `[InlineWrapper]` — branded primitives

Struct with a single primitive field → **branded type** with zero runtime overhead.
Perfect for strongly-typed IDs.

```csharp
[InlineWrapper]
public readonly record struct UserId(string Value)
{
    public static UserId New() => new(Guid.NewGuid().ToString("N"));
}
```

Becomes:

```typescript
export type UserId = string & { readonly __brand: "UserId" };
export namespace UserId {
  export function create(value: string): UserId { return value as UserId; }
  export function newId(): UserId { return crypto.randomUUID().replace(/-/g, "") as UserId; }
}
```

At runtime `UserId` is literally a `string` — no wrapper object. But the type
system prevents you from passing a plain string where a `UserId` is expected.

### `[PlainObject]` — record → interface

Emits a record or class as a TypeScript **interface** instead of a class. No
`equals`, no `hashCode`, no `with`, no constructor call site lowering.

Useful for DTOs that flow through `JSON.stringify`/`parse` or HTTP boundaries.

```csharp
[PlainObject]
public record CreateUserDto(string Name, string Email);
```

Becomes:

```typescript
export interface CreateUserDto {
  name: string;
  email: string;
}
```

And at call sites, `new CreateUserDto("Alice", "a@example.com")` becomes
`{ name: "Alice", email: "a@example.com" }`.

### `[ExportedAsModule]`

Applied to a **static class**: instead of emitting a class, each method becomes a
top-level exported function in the generated `.ts` file.

```csharp
[ExportedAsModule]
public static class MathHelpers
{
    public static int Square(int x) => x * x;
    public static int Cube(int x) => x * x * x;
}
```

Becomes:

```typescript
export function square(x: number): number {
  return x * x;
}

export function cube(x: number): number {
  return x * x * x;
}
```

No `MathHelpers` class in the output — just loose functions. Idiomatic in JS
modules.

### `[GenerateGuard]`

Generates a TypeScript type guard function `isTypeName(value: unknown): value is TypeName`
next to the type. Validates structure at runtime.

```csharp
[Transpile, GenerateGuard]
public record Money(decimal Amount, string Currency);
```

Emits both the `Money` class AND:

```typescript
export function isMoney(value: unknown): value is Money {
  return (
    value instanceof Money ||
    (typeof value === "object" && value !== null &&
      "amount" in value && value.amount instanceof Decimal &&
      "currency" in value && typeof value.currency === "string")
  );
}
```

Guards are auto-imported wherever they're needed (e.g., in generated
overload dispatchers).

### `[EmitInFile("name")]`

Co-locates multiple types in a single `.ts` file. Types in the same C# namespace
with the same `EmitInFile("foo")` name share `foo.ts`.

```csharp
[PlainObject, EmitInFile("todos")]
public record StoredTodo(string Id, TodoItem Item);

[PlainObject, EmitInFile("todos")]
public record CreateTodoDto(string Title, Priority Priority);

[EmitInFile("todos")]
public class TodoStore { /* … */ }
```

All three land in `todos.ts`. Consumers that import any of them get
`import { … } from "…/todos"` automatically.

## Module-level features

### `[ModuleEntryPoint]`

Applied to a static method. The method **body** becomes top-level code in the
generated TypeScript module — no function wrapper. Useful for app bootstrap code.

```csharp
[ExportedAsModule]
public static class Program
{
    [ModuleEntryPoint]
    public static void Main()
    {
        var app = new Hono();
        app.Get("/", c => c.Text("hello"));
    }
}
```

Becomes:

```typescript
const app = new Hono();
app.get("/", (c) => c.text("hello"));
```

### `[ExportVarFromBody("name", AsDefault = true)]`

Combines with `[ModuleEntryPoint]` to promote a local variable to a module
export (optionally `export default`).

```csharp
[ExportedAsModule]
public static class Program
{
    [ModuleEntryPoint, ExportVarFromBody("app", AsDefault = true)]
    public static void Main()
    {
        var app = new Hono();
        // … wire routes
    }
}
```

Becomes:

```typescript
const app = new Hono();
// … routes

export default app;
```

## External bindings

### `[Import(name, from, Version = "^x.y.z", AsDefault = false)]`

Declares a type that's provided by an **external npm package**, not by your C#
code. The transpiler emits `import { Name } from "package-name"` wherever this
type is used, and adds the package to the generated `package.json#dependencies`.

```csharp
[Import(name: "Hono", from: "hono", Version = "^4.6.0")]
public class Hono
{
    [Name("get")]
    public void Get(string path, Func<IHonoContext, IHonoContext> handler) =>
        throw new NotSupportedException();
}
```

Set `AsDefault = true` for default imports (`import Foo from "package"`).

### `[assembly: ExportFromBcl(typeof(BclType), FromPackage = "npm-pkg", ExportedName = "TsName", Version = "^x.y.z")]`

Assembly-level mapping from a .NET BCL type to an npm package. Used by the
`Metano` package itself to wire up default mappings like
`decimal` → `Decimal` from `decimal.js`.

```csharp
[assembly: ExportFromBcl(
    typeof(decimal),
    FromPackage = "decimal.js",
    ExportedName = "Decimal",
    Version = "^10.6.0")]
```

### `[Emit("$0.foo($1, $2)")]`

Inlines JavaScript at the call site, with `$0` = the receiver and `$1, $2, …` =
arguments. Escape hatch for when you need a specific JS idiom that isn't worth
modeling in C#.

```csharp
[Emit("$0.replace(/-/g, \"\")")]
public static string StripDashes(this string value) =>
    throw new NotSupportedException();
```

At the call site `text.StripDashes()` emits `text.replace(/-/g, "")`.

## Cross-project packaging

### `[assembly: EmitPackage("name", Version = "^x.y.z")]`

Declares the npm package name for this assembly's transpiled output. Used by
cross-project resolution: when another project references this assembly, imports
resolve to `"name/subpath"` instead of `"#/...".

```csharp
[assembly: TranspileAssembly]
[assembly: EmitPackage("@acme/domain", Version = "^1.0.0")]
```

The `Version` field is optional — if omitted, the consumer uses `workspace:*` for
monorepo siblings or the assembly's `Identity.Version`.

## Declarative BCL mappings

### `[assembly: MapMethod(typeof(T), "MethodName", JsMethod = "jsName")]`

Maps a .NET BCL method to a JS method. Used by the `Metano.Runtime` assembly to
define things like `List<T>.Add` → `push`, `List<T>.Contains` → `includes`.

```csharp
[assembly: MapMethod(typeof(List<>), nameof(List<int>.Add), JsMethod = "push")]
```

Alternative: `JsTemplate` for full control:

```csharp
[assembly: MapMethod(
    typeof(Enumerable),
    nameof(Enumerable.Where),
    WrapReceiver = "Enumerable.from",
    JsTemplate = "$this.where($0)")]
```

Placeholders:

- `$this` — the receiver
- `$0, $1, …` — arguments
- `$T0, $T1, …` — generic type arguments

### `[assembly: MapProperty(typeof(T), "PropertyName", JsProperty = "jsName")]`

Same idea for properties.

```csharp
[assembly: MapProperty(typeof(List<>), nameof(List<int>.Count), JsProperty = "length")]
```

## See also

- [BCL Type Mappings](bcl-mappings.md) — how C# types map to TypeScript
- [Cross-Project References](cross-package.md) — deeper dive into `[EmitPackage]`
