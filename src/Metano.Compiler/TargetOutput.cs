using Metano.Compiler.Diagnostics;

namespace Metano.Compiler;

/// <summary>
/// The result of running a transpiler target: a list of files to emit and any diagnostics
/// collected during transformation.
/// </summary>
public sealed record TargetOutput(
    IReadOnlyList<GeneratedFile> Files,
    IReadOnlyList<MetanoDiagnostic> Diagnostics
);
