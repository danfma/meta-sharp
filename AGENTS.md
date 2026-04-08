# Repository Guidelines

## Project Structure & Module Organization
`MetaSharp.slnx` is the entry point for the .NET solution. `MetaSharp/` holds the C# attributes (in the `MetaSharp.Annotations` namespace, under `MetaSharp/Annotations/`) that control transpilation. `MetaSharp.Compiler/` is the **target-agnostic core** (`ITranspilerTarget`, `TranspilerHost`, diagnostics, shared symbol helpers). `MetaSharp.Compiler.TypeScript/` is the TypeScript target — it depends on the core, ships the TypeScript AST + printer + the 30+ focused handlers under `Transformation/`, and exposes the `metasharp-typescript` CLI. `MetaSharp.Tests/` holds TUnit coverage plus golden files in `MetaSharp.Tests/Expected/`. `SampleTodo/` and `SampleIssueTracker/` are the end-to-end C# samples. The `js/` workspace contains `meta-sharp-runtime/` for shared Bun/TypeScript runtime helpers and `sample-todo/` / `sample-issue-tracker/` for generated TypeScript and Bun tests.

## Build, Test, and Development Commands
Use the repository root for .NET commands and Bun workspace folders for JavaScript commands.

- `dotnet build` builds the full solution.
- `dotnet run --project MetaSharp.Tests/` runs the .NET test suite. Use this instead of `dotnet test` on .NET 10/TUnit.
- `dotnet run --project MetaSharp.Compiler.TypeScript/ -- -p SampleTodo/SampleTodo.csproj -o js/sample-todo/src --clean` regenerates the sample TypeScript output.
- `dotnet csharpier .` formats C# sources.
- `cd js/meta-sharp-runtime && bun run build` builds the runtime with `tsgo`.
- `cd js/meta-sharp-runtime && bun test` runs runtime tests.
- `cd js/sample-todo && bun run build && bun test` validates generated sample output.

## Coding Style & Naming Conventions
C# uses 4-space indentation, file-scoped namespaces where practical, `PascalCase` for types/members, and nullable-enabled code. Keep new compiler logic near the existing `Transformation/` and `TypeScript/` layers. TypeScript in `js/` follows the current 2-space style, ESM imports, strict compiler settings, and `PascalCase` classes with `camelCase` methods. Prefer `CSharpier` for C# formatting; keep TypeScript consistent with the existing workspace style and `tsconfig` strictness.

## Testing Guidelines
Add .NET tests under `MetaSharp.Tests/` using the existing `*TranspileTests.cs` naming pattern. Store expected emitted `.ts` files in `MetaSharp.Tests/Expected/` when output snapshots matter. For runtime and sample validation, add Bun tests as `*.test.ts` or the generated equivalent already used in `js/sample-todo/dist/`. Run both the .NET suite and relevant Bun tests before opening a PR.

## Commit & Pull Request Guidelines
The current history uses Conventional Commit style, for example `feat: initial MetaSharp standalone repository`; continue with prefixes like `feat:`, `fix:`, `refactor:`, or `test:`. PRs should include a short summary, linked issue if available, the commands you ran, and sample emitted TypeScript or diff notes when compiler output changes.

## Agent Notes
For AI-assisted work, read `CLAUDE.md` first. Prefer Bun over npm/yarn/pnpm, do not use `dotnet test`, and regenerate `js/sample-todo/src` through the compiler instead of hand-editing generated output.
