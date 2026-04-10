using Microsoft.CodeAnalysis;

namespace Metano.Compiler;

/// <summary>
/// Implemented by each language-specific backend (TypeScript, Dart, Kotlin, …).
/// The host (TranspilerHost) takes care of opening the C# project, running the Roslyn
/// compilation, and writing files to disk; the target only needs to consume the
/// Compilation and produce a TargetOutput.
/// </summary>
public interface ITranspilerTarget
{
    /// <summary>Short name used in CLI/log messages (e.g., "typescript", "dart").</summary>
    string Name { get; }

    /// <summary>
    /// Transforms a Roslyn compilation into a set of generated files plus diagnostics.
    /// Implementations must NOT perform file I/O — the host writes the returned files.
    /// </summary>
    TargetOutput Transform(Compilation compilation);
}
