using System.Diagnostics;
using ConsoleAppFramework;
using MetaSharp.Transformation;
using MetaSharp.TypeScript;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace MetaSharp;

public class Commands
{
    /// <summary>
    /// Transpile C# types to TypeScript.
    /// </summary>
    /// <param name="project">-p, Path to the C# project file (.csproj)</param>
    /// <param name="output">-o, Output directory for generated TypeScript files</param>
    /// <param name="time">-t, Show compilation and transpilation timings</param>
    /// <param name="clean">-c, Clean output directory before generating</param>
    [Command("")]
    public async Task Transpile(string project, string output, bool time = false, bool clean = false)
    {
        var projectPath = Path.GetFullPath(project);
        var outputDir = Path.GetFullPath(output);

        if (!File.Exists(projectPath))
        {
            Console.Error.WriteLine($"Project not found: {projectPath}");
            Environment.Exit(1);
            return;
        }

        var totalSw = Stopwatch.StartNew();

        Console.WriteLine($"MetaSharp: Loading project {Path.GetFileName(projectPath)}...");

        using var workspace = MSBuildWorkspace.Create();
        workspace.RegisterWorkspaceFailedHandler(e =>
        {
            if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                Console.Error.WriteLine($"  Workspace error: {e.Diagnostic.Message}");
        });

        var compileSw = Stopwatch.StartNew();

        var proj = await workspace.OpenProjectAsync(projectPath);
        var compilation = await proj.GetCompilationAsync();

        compileSw.Stop();

        if (compilation is null)
        {
            Console.Error.WriteLine("Failed to compile project.");
            Environment.Exit(1);
            return;
        }

        var errors = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        if (errors.Count > 0)
        {
            Console.Error.WriteLine($"Compilation has {errors.Count} error(s):");
            foreach (var error in errors.Take(10))
                Console.Error.WriteLine($"  {error}");
            Environment.Exit(1);
            return;
        }

        if (time)
            Console.WriteLine($"  Compilation: {compileSw.ElapsedMilliseconds}ms");

        var transpileSw = Stopwatch.StartNew();

        var transformer = new TypeTransformer(compilation);
        var files = transformer.TransformAll();

        transpileSw.Stop();

        if (time)
            Console.WriteLine($"  Transpilation: {transpileSw.ElapsedMilliseconds}ms");

        if (files.Count == 0)
        {
            Console.WriteLine("MetaSharp: No transpilable types found.");
            return;
        }

        var emitSw = Stopwatch.StartNew();

        if (clean && Directory.Exists(outputDir))
        {
            Directory.Delete(outputDir, recursive: true);
            Console.WriteLine($"  Cleaned: {outputDir}");
        }

        Directory.CreateDirectory(outputDir);
        var printer = new Printer();

        foreach (var file in files)
        {
            var content = printer.Print(file);
            var filePath = Path.Combine(outputDir, file.FileName.Replace('/', Path.DirectorySeparatorChar));
            var fileDir = Path.GetDirectoryName(filePath);
            if (fileDir is not null) Directory.CreateDirectory(fileDir);
            await File.WriteAllTextAsync(filePath, content);
            Console.WriteLine($"  Generated: {file.FileName}");
        }

        emitSw.Stop();
        totalSw.Stop();

        if (time)
        {
            Console.WriteLine($"  Emit: {emitSw.ElapsedMilliseconds}ms");
            Console.WriteLine($"  Total: {totalSw.ElapsedMilliseconds}ms");
        }

        Console.WriteLine($"MetaSharp: {files.Count} file(s) generated in {outputDir}");
    }
}
