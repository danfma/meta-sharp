using System.Text.Json.Nodes;
using MetaSharp.Compiler.Diagnostics;
using MetaSharp.TypeScript.AST;

namespace MetaSharp.Tests;

/// <summary>
/// Tests for the <c>[assembly: EmitPackage(name)]</c> integration with
/// <see cref="PackageJsonWriter"/>. The attribute is the authoritative source for the
/// generated package.json's <c>name</c> field; divergence with an existing file emits
/// MS0007 (warning) and the attribute value still wins because cross-package import
/// resolution depends on it.
/// </summary>
public class EmitPackageTests
{
    [Test]
    public async Task NoExistingFile_AuthoritativeNameWritten()
    {
        var tempDir = CreateTempDir();
        var srcDir = Path.Combine(tempDir, "src");
        Directory.CreateDirectory(srcDir);

        var diags = PackageJsonWriter.UpdateOrCreate(
            tempDir, srcDir, files: [], authoritativePackageName: "@scope/cool-pkg");

        var pkg = ReadJson(tempDir);
        await Assert.That(pkg["name"]?.GetValue<string>()).IsEqualTo("@scope/cool-pkg");
        await Assert.That(diags.Count).IsEqualTo(0);

        Directory.Delete(tempDir, recursive: true);
    }

    [Test]
    public async Task ExistingFileWithMatchingName_NoWarning()
    {
        var tempDir = CreateTempDir();
        var srcDir = Path.Combine(tempDir, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(
            Path.Combine(tempDir, "package.json"),
            """{ "name": "sample-todo", "private": true }""");

        var diags = PackageJsonWriter.UpdateOrCreate(
            tempDir, srcDir, files: [], authoritativePackageName: "sample-todo");

        var pkg = ReadJson(tempDir);
        await Assert.That(pkg["name"]?.GetValue<string>()).IsEqualTo("sample-todo");
        await Assert.That(diags.Count).IsEqualTo(0);

        Directory.Delete(tempDir, recursive: true);
    }

    [Test]
    public async Task ExistingFileWithDivergentName_WarnsAndOverwrites()
    {
        var tempDir = CreateTempDir();
        var srcDir = Path.Combine(tempDir, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(
            Path.Combine(tempDir, "package.json"),
            """{ "name": "old-name", "private": true }""");

        var diags = PackageJsonWriter.UpdateOrCreate(
            tempDir, srcDir, files: [], authoritativePackageName: "new-name");

        var pkg = ReadJson(tempDir);
        // Authoritative wins.
        await Assert.That(pkg["name"]?.GetValue<string>()).IsEqualTo("new-name");
        // And the writer reported MS0007.
        await Assert.That(diags.Any(d => d.Code == DiagnosticCodes.CrossPackageResolution)).IsTrue();
        await Assert.That(diags.Any(d => d.Severity == MetaSharpDiagnosticSeverity.Warning)).IsTrue();

        Directory.Delete(tempDir, recursive: true);
    }

    [Test]
    public async Task NoAuthoritativeName_PreservesExisting()
    {
        var tempDir = CreateTempDir();
        var srcDir = Path.Combine(tempDir, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(
            Path.Combine(tempDir, "package.json"),
            """{ "name": "hand-written", "private": true }""");

        var diags = PackageJsonWriter.UpdateOrCreate(
            tempDir, srcDir, files: [], authoritativePackageName: null);

        var pkg = ReadJson(tempDir);
        await Assert.That(pkg["name"]?.GetValue<string>()).IsEqualTo("hand-written");
        await Assert.That(diags.Count).IsEqualTo(0);

        Directory.Delete(tempDir, recursive: true);
    }

    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"metasharp-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static JsonObject ReadJson(string dir) =>
        (JsonNode.Parse(File.ReadAllText(Path.Combine(dir, "package.json"))) as JsonObject)!;
}
