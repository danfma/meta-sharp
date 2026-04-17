using Metano.Compiler.IR;

namespace Metano.Compiler;

/// <summary>
/// Implemented by each source-language frontend (C# via Roslyn today,
/// potentially others in the future). Given a project entry point, the
/// frontend loads the source, discovers transpilable declarations, runs
/// the semantic extraction pipeline, and produces a target-agnostic
/// <see cref="IrCompilation"/> every backend can consume.
/// <para>
/// A frontend is stateless across invocations — one call per transpile
/// run. The resulting <see cref="IrCompilation"/> carries all state the
/// backend needs; no shared context reaches over.
/// </para>
/// </summary>
public interface ISourceFrontend
{
    /// <summary>Short identifier for CLI / log messages (e.g.,
    /// <c>"csharp"</c>).</summary>
    string Name { get; }

    /// <summary>Asynchronously loads + extracts the project at
    /// <paramref name="projectPath"/>.</summary>
    /// <param name="projectPath">Path to the entry source artifact (e.g.,
    /// a <c>.csproj</c> for the C# frontend).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The extracted compilation, or <c>null</c> if loading
    /// failed. Frontend-level failures (project not found, compile
    /// errors) surface via diagnostics and a null result.</returns>
    Task<IrCompilation?> ExtractAsync(string projectPath, CancellationToken ct = default);
}
