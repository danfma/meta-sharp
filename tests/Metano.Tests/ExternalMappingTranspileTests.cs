namespace Metano.Tests;

public class ExternalMappingTranspileTests
{
    // ─── [ExportFromBcl] ────────────────────────────────────

    [Test]
    public async Task ExportFromBcl_OverridesHardcodedMapping()
    {
        var result = TranspileHelper.Transpile(
            """
            [assembly: ExportFromBcl(typeof(decimal), FromPackage = "decimal.js", ExportedName = "Decimal")]

            [Transpile]
            public record Price(decimal Amount, string Currency);
            """
        );

        var output = result["price.ts"];
        await Assert.That(output).Contains("amount: Decimal");
        await Assert.That(output).Contains("from \"decimal.js\"");
    }

    [Test]
    public async Task Decimal_HasBuiltInMappingToDecimalJs()
    {
        // Metano ships a default [ExportFromBcl] for `decimal` in
        // Metano/Runtime/Decimal.cs, so user code that uses decimal automatically
        // imports `Decimal` from `decimal.js` without any per-project declaration.
        // The user is responsible for adding `decimal.js` to their package.json.
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public record Price(decimal Amount, string Currency);
            """
        );

        var output = result["price.ts"];
        await Assert.That(output).Contains("amount: Decimal");
        await Assert.That(output).Contains("from \"decimal.js\"");
    }

    // ─── [Import] ───────────────────────────────────────────

    [Test]
    public async Task Import_TypeNotGenerated()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile, Import("Decimal", from: "decimal.js")]
                public class ExternalDecimal { }

                [Transpile]
                public record Price(int Cents);
            }
            """
        );

        await Assert.That(result).DoesNotContainKey("external-decimal.ts");
        await Assert.That(result).ContainsKey("price.ts");
    }

    [Test]
    public async Task Import_ReferencedTypeGeneratesCorrectImport()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile, Import("Moment", from: "moment")]
                public class Moment { }

                [Transpile]
                public record Event(string Name, Moment When);
            }
            """
        );

        var output = result["event.ts"];
        await Assert.That(output).Contains("from \"moment\"");
        await Assert.That(output).Contains("when: Moment");
    }

    // ─── [Emit] ─────────────────────────────────────────────

    [Test]
    public async Task Emit_MethodNotGeneratedInOutput()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile, Erasable]
            public static class Helpers
            {
                [Emit("$0.toFixed($1)")]
                public static extern string ToFixed(decimal value, int digits);

                public static int Double(int x) => x * 2;
            }
            """
        );

        var output = result["helpers.ts"];
        await Assert.That(output).DoesNotContain("toFixed");
        await Assert.That(output).Contains("double");
    }

    [Test]
    public async Task Emit_InlinedAtCallSite()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile, Erasable]
            public static class Utils
            {
                [Emit("typeof $0")]
                public static extern string TypeOf(object value);

                public static string CheckType(object x)
                {
                    return TypeOf(x);
                }
            }
            """
        );

        var output = result["utils.ts"];
        await Assert.That(output).Contains("typeof x");
    }

    [Test]
    public async Task Emit_MultipleArguments()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile, Erasable]
            public static class Fmt
            {
                [Emit("$0.slice($1, $2)")]
                public static extern string Slice(string str, int start, int end);

                public static string Mid(string s)
                {
                    return Slice(s, 1, 3);
                }
            }
            """
        );

        var output = result["fmt.ts"];
        await Assert.That(output).Contains("s.slice(1, 3)");
    }

    [Test]
    public async Task Emit_TypeArgumentPlaceholderEmbedsTypeName()
    {
        // `$T0` substitutes the call-site's first generic type argument verbatim
        // into the lowered template — used to embed component class references
        // like `createElement(CounterApp, props)` without a typeof expression.
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile]
                public class Widget { }

                [Transpile, Erasable]
                public static class Factory
                {
                    [Emit("make($T0, $0)")]
                    public static extern object Of<T>(object props);
                }

                [Transpile]
                public static class Caller
                {
                    [ModuleEntryPoint]
                    public static void Run()
                    {
                        var x = Factory.Of<Widget>(new { });
                    }
                }
            }
            """
        );

        var caller = result["caller.ts"];
        await Assert.That(caller).Contains("make(Widget,");
        await Assert.That(caller).Contains("import { Widget }");
    }

    [Test]
    public async Task Emit_WithImport_DrivesConsumerImportLine()
    {
        // [Import] on the same method as [Emit] tells the consumer file which
        // identifier the template body references and where it lives. Without
        // this thread, the lowered call would name `createElement` but no
        // import line would resolve it.
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile, Erasable]
                public static class H
                {
                    [Emit("createElement($T0, $0)"), Import(name: "createElement", from: "react")]
                    public static extern object Of<T>(object props);
                }

                [Transpile]
                public class Comp { }

                [Transpile]
                public static class Caller
                {
                    [ModuleEntryPoint]
                    public static void Run()
                    {
                        var x = H.Of<Comp>(new { });
                    }
                }
            }
            """
        );

        var caller = result["caller.ts"];
        await Assert.That(caller).Contains("createElement(Comp,");
        await Assert.That(caller).Contains("from \"react\"");
        await Assert.That(caller).Contains("createElement");
    }

    // ─── [Import] on methods/properties ─────────────────────

    [Test]
    public async Task Import_StaticMethod_NoStubEmitted()
    {
        // A method-level [Import] declares an external binding — the body is
        // unreachable noise. The declaring class should not emit a function
        // declaration for it.
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile, Erasable]
                public static class Inferno
                {
                    [Import(name: "createElement", from: "inferno-create-element"), Name("createElement")]
                    public static object H(string tag, object props) => throw new System.NotSupportedException();
                }

                [Transpile]
                public static class Caller
                {
                    [ModuleEntryPoint]
                    public static void Run()
                    {
                        var x = Inferno.H("div", new { });
                    }
                }
            }
            """
        );

        // Inferno is fully erased — no inferno.ts file.
        await Assert.That(result).DoesNotContainKey("inferno.ts");
    }

    [Test]
    public async Task Import_StaticMethod_LowersToDirectCallAtCallSite()
    {
        // Plain [Import] (no [Emit] template) — the call site lowers to a
        // direct invocation of the imported identifier, and the consumer file
        // gets the matching import line auto-emitted.
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile, Erasable]
                public static class Inferno
                {
                    [Import(name: "createElement", from: "inferno-create-element")]
                    public static object H(string tag, object props) =>
                        throw new System.NotSupportedException();
                }

                [Transpile]
                public static class Caller
                {
                    [ModuleEntryPoint]
                    public static void Run()
                    {
                        var x = Inferno.H("div", new { });
                    }
                }
            }
            """
        );

        var caller = result["caller.ts"];
        await Assert.That(caller).Contains("createElement(\"div\",");
        await Assert.That(caller).Contains("from \"inferno-create-element\"");
        await Assert.That(caller).DoesNotContain(".H(");
    }

    [Test]
    public async Task Import_MethodOnTranspilableClass_NotEmittedAsClassMember()
    {
        // [Import] on an ordinary class member should not surface as a stub
        // method on the emitted class — the call site lowers separately.
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile]
                public class Wrapper
                {
                    [Import(name: "doIt", from: "lib")]
                    public static object Run(int n) => throw new System.NotSupportedException();
                }
            }
            """
        );

        var output = result["wrapper.ts"];
        // No function/method declaration for the [Import] member.
        await Assert.That(output).DoesNotContain("doIt(n: number)");
        await Assert.That(output).DoesNotContain("Run(n: number)");
    }
}
