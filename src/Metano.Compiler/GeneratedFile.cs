namespace Metano.Compiler;

/// <summary>
/// Represents a single file produced by a transpiler target. The path is relative to the
/// configured output directory; the content is the file's text serialized by the target.
/// </summary>
public sealed record GeneratedFile(string RelativePath, string Content);
