// Default BCL → JavaScript mapping for `System.Decimal`. Lowers to the `Decimal` class
// from the `decimal.js` package — a precise arbitrary-precision number type that
// preserves the C# `decimal` semantics (no IEEE-754 rounding errors). Without this
// mapping, `decimal` would silently fall through to the primitive switch and become
// the JS `number` primitive, which is a footgun for any monetary or scientific code.
//
// **Consumer requirement:** add `decimal.js` to your `package.json` dependencies.
// MetaSharp does NOT manage the JS package install — only the import statement in the
// generated TypeScript points at the package.
//
// This file only declares the **type-level** mapping. Literal lowering (1.5m →
// new Decimal("1.5")) and operator lowering (a + b → a.plus(b) for decimal operands)
// live inside the compiler itself, since they need access to the semantic model.

using MetaSharp.Annotations;

[assembly: ExportFromBcl(typeof(decimal),
    ExportedName = "Decimal",
    FromPackage = "decimal.js")]
