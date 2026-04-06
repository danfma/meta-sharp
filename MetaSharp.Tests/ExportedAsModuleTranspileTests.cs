namespace MetaSharp.Tests;

public class ExportedAsModuleTranspileTests
{
    [Test]
    public async Task StaticClass_EmitsTopLevelFunctions()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile, ExportedAsModule]
            public static class MathUtils
            {
                public static int Add(int a, int b) => a + b;

                public static string Greet(string name) => $"Hello, {name}!";
            }
            """
        );

        var expected = TranspileHelper.ReadExpected("ExportedAsModule.ts");
        await Assert.That(result["MathUtils.ts"]).IsEqualTo(expected);
    }

    [Test]
    public async Task StaticClass_NoClassWrapper()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile, ExportedAsModule]
            public static class Helpers
            {
                public static int Double(int x) => x * 2;
            }
            """
        );

        var output = result["Helpers.ts"];
        await Assert.That(output).DoesNotContain("class Helpers");
        await Assert.That(output).Contains("export function");
    }

    [Test]
    public async Task StaticClass_AsyncMethodsWork()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile, ExportedAsModule]
            public static class Api
            {
                public static async Task<string> FetchData(string url)
                {
                    return url;
                }
            }
            """
        );

        var expected = TranspileHelper.ReadExpected("ExportedAsModuleAsync.ts");
        await Assert.That(result["Api.ts"]).IsEqualTo(expected);
    }

    [Test]
    public async Task StaticClass_IgnoredMembersSkipped()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile, ExportedAsModule]
            public static class Utils
            {
                public static int Visible() => 1;

                [Ignore]
                public static int Hidden() => 2;
            }
            """
        );

        var output = result["Utils.ts"];
        await Assert.That(output).Contains("visible");
        await Assert.That(output).DoesNotContain("hidden");
    }

    [Test]
    public async Task StaticClass_NameAttributeRenamesFunction()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile, ExportedAsModule]
            public static class Ops
            {
                [Name("sum")]
                public static int Add(int a, int b) => a + b;
            }
            """
        );

        var output = result["Ops.ts"];
        await Assert.That(output).Contains("export function sum(");
        await Assert.That(output).DoesNotContain("export function add(");
    }

    [Test]
    public async Task StaticClass_NoEqualsHashCodeWith()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile, ExportedAsModule]
            public static class Pure
            {
                public static int Identity(int x) => x;
            }
            """
        );

        var output = result["Pure.ts"];
        await Assert.That(output).DoesNotContain("equals");
        await Assert.That(output).DoesNotContain("hashCode");
        await Assert.That(output).DoesNotContain("with");
    }
}
