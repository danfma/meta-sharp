using MetaSharp.Compiler;
using MetaSharp.Transformation;
using MetaSharp.TypeScript;
using MetaSharp.TypeScript.AST;
using Microsoft.CodeAnalysis;

namespace MetaSharp;

/// <summary>
/// <see cref="ITranspilerTarget"/> implementation for the TypeScript backend.
/// Wraps the legacy <see cref="TypeTransformer"/> + <see cref="Printer"/> pipeline.
/// </summary>
/// <remarks>
/// The list of generated <see cref="TsSourceFile"/>s is exposed via <see cref="LastSourceFiles"/>
/// after a Transform call so the caller can perform target-specific post-processing
/// (e.g., writing a package.json with imports/exports/sideEffects derived from the AST).
/// </remarks>
public sealed class TypeScriptTarget : ITranspilerTarget
{
    public string Name => "typescript";

    /// <summary>
    /// The TS AST source files produced by the most recent <see cref="Transform"/> call.
    /// Empty until Transform is invoked.
    /// </summary>
    public IReadOnlyList<TsSourceFile> LastSourceFiles { get; private set; } = [];

    /// <summary>
    /// The package name read from <c>[assembly: EmitPackage(name)]</c> on the compiled
    /// assembly, or null when the attribute isn't present. Used by the CLI driver to
    /// pass an authoritative name to <see cref="PackageJsonWriter"/>.
    /// </summary>
    public string? LastEmitPackageName { get; private set; }

    public TargetOutput Transform(Compilation compilation)
    {
        var transformer = new TypeTransformer(compilation);
        var sourceFiles = transformer.TransformAll();
        LastSourceFiles = sourceFiles;
        // Read [EmitPackage] for the JavaScript target (enum value 0).
        LastEmitPackageName = SymbolHelper.GetEmitPackage(compilation.Assembly, targetEnumValue: 0);

        var printer = new Printer();
        var generated = new List<GeneratedFile>(sourceFiles.Count);
        foreach (var file in sourceFiles)
            generated.Add(new GeneratedFile(file.FileName, printer.Print(file)));

        return new TargetOutput(generated, transformer.Diagnostics);
    }
}
