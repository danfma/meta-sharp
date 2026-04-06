# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MetaSharp is a C# → TypeScript transpiler powered by Roslyn. It reads C# projects, transforms annotated types into a TypeScript AST, and prints formatted .ts files. It includes a lazy LINQ runtime and specialized type checks for overload dispatch.

## Commands

### .NET

```sh
dotnet build                                          # build entire solution
dotnet run --project MetaSharp.Tests/                  # run tests (TUnit — use dotnet run, not dotnet test)
dotnet run --project MetaSharp.Compiler/ -- \
  -p SampleTodo/SampleTodo.csproj \
  -o js/sample-todo/src --clean                       # transpile SampleTodo to TypeScript
dotnet csharpier .                                    # format C# code
```

TUnit on .NET 10 requires `dotnet run` instead of `dotnet test`.

### JavaScript/TypeScript (Bun)

```sh
cd js/meta-sharp-runtime && bun run build             # TypeScript build (tsgo)
cd js/meta-sharp-runtime && bun test                  # run runtime tests (86 tests)
cd js/sample-todo && bun run build                    # TS build of generated code
cd js/sample-todo && bun test                         # end-to-end tests (17 tests)
```

Always use **Bun** — never npm, yarn, or pnpm.

## Architecture

```
MetaSharp.slnx
├── MetaSharp.Annotations/     # Attributes for transpilation control (13 attributes)
├── MetaSharp.Compiler/        # C# → TypeScript transpiler (Roslyn-based)
│   ├── Transformation/        # TypeTransformer, ExpressionTransformer, TypeMapper, BclMapper
│   ├── TypeScript/AST/        # ~60 record types representing TS AST nodes
│   ├── TypeScript/Printer.cs  # AST → formatted text
│   └── Commands.cs            # CLI (ConsoleAppFramework)
├── MetaSharp.Tests/           # 166 TUnit tests with inline C# compilation
│   └── Expected/              # Expected .ts output files
├── SampleTodo/                # Sample C# project for end-to-end validation
└── js/                        # Bun workspace
    ├── meta-sharp-runtime/    # @meta-sharp/runtime (HashCode, LINQ, type checks)
    └── sample-todo/           # Generated TS from SampleTodo + bun tests
```

### Pipeline

C# source + MetaSharp attributes → Roslyn SemanticModel → TypeScript AST → Printer → .ts files

### MetaSharp Annotations

| Attribute | Target | Purpose |
|-----------|--------|---------|
| `[Transpile]` | Type | Marks type for transpilation |
| `[assembly: TranspileAssembly]` | Assembly | Transpiles all public types (opt-out with `[NoTranspile]`) |
| `[NoTranspile]` | Type | Excludes from transpilation |
| `[StringEnum]` | Enum | Generates TS string union instead of numeric enum |
| `[Name("x")]` | Any | Overrides name in TS output |
| `[Ignore]` | Member | Omits member from output |
| `[ExportedAsModule]` | Static class | Emits top-level functions instead of class |
| `[GenerateGuard]` | Type | Generates `isTypeName()` type guard function |
| `[ExportFromBcl]` | Assembly | Maps BCL type to JS package |
| `[Import]` | Type/Method | Declares external JS module dependency |
| `[Emit("$0.foo($1)")]` | Method | Inlines JS at call site with argument placeholders |

### Tests

Tests use `TranspileHelper.Transpile(csharpSource)` which compiles C# inline, runs the transformer, and returns `filename → TS content`. Expected output files live in `MetaSharp.Tests/Expected/`.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| .NET SDK | 10.0 (C# 14, preview features) |
| Transpiler | Roslyn 5.3.0 (Microsoft.CodeAnalysis) |
| CLI | ConsoleAppFramework |
| Testing (.NET) | TUnit |
| Testing (TS) | bun:test |
| Formatting | CSharpier (.NET) |
| Runtime | @meta-sharp/runtime (Bun/TypeScript) |
| Package management | Central Package Management (Directory.Packages.props) |

## Build Configuration

- `global.json` — SDK 10.0.0, rollForward latestMinor
- `Directory.Build.props` — TreatWarningsAsErrors, ImplicitUsings, Nullable, LangVersion preview
- `Directory.Packages.props` — Central Package Management
- `specs/next-steps.md` — Full feature backlog and roadmap
