using MetaSharp.Compiler.Diagnostics;

namespace MetaSharp.Compiler;

/// <summary>
/// The result of running a transpiler target: a list of files to emit and any diagnostics
/// collected during transformation.
/// </summary>
public sealed record TargetOutput(
    IReadOnlyList<GeneratedFile> Files,
    IReadOnlyList<MetaSharpDiagnostic> Diagnostics
);
