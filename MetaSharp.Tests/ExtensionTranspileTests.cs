namespace MetaSharp.Tests;

public class ExtensionTranspileTests
{
    // ─── Classic extension methods ──────────────────────────

    [Test]
    public async Task ClassicExtension_ReceiverBecomesFirstParam()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public static class StringExt
            {
                public static string Upper(this string s) => s.ToUpper();
            }
            """
        );

        var output = result["StringExt.ts"];
        await Assert.That(output).Contains("export function upper(s: string): string");
    }

    [Test]
    public async Task ClassicExtension_MultipleParams()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public static class MathExt
            {
                public static int Add(this int x, int y) => x + y;
            }
            """
        );

        var output = result["MathExt.ts"];
        await Assert.That(output).Contains("export function add(x: number, y: number): number");
    }

    [Test]
    public async Task ClassicExtension_AutoDetectedAsModule()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public static class Helpers
            {
                public static int Double(this int x) => x * 2;
                public static int Triple(this int x) => x * 3;
            }
            """
        );

        var output = result["Helpers.ts"];
        // Should be module-style (top-level functions), not a class
        await Assert.That(output).DoesNotContain("class Helpers");
        await Assert.That(output).Contains("export function");
    }

    [Test]
    public async Task ClassicExtension_GenericReceiver()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public static class EnumerableExt
            {
                public static bool IsEmpty<T>(this System.Collections.Generic.IEnumerable<T> source)
                {
                    return false;
                }
            }
            """
        );

        var output = result["EnumerableExt.ts"];
        await Assert.That(output).Contains("function isEmpty<T>(source: T[]): boolean");
    }

    // ─── C# 14 extension blocks ─────────────────────────────
    // NOTE: C# 14 extension blocks are supported by the compiler when processing real .csproj files
    // (MSBuildWorkspace resolves language version from the project). The inline TranspileHelper
    // compilation may not fully support them. These tests are pending proper compilation setup.
    // The detection (HasExtensionMembers) and transformation (TransformAsModule) are implemented
    // and work with MSBuildWorkspace-based compilation.

    // TODO: Add C# 14 extension block tests when TranspileHelper supports full C# 14 compilation
}
