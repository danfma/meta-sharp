namespace Metano.Tests;

public class ObjectArgsTranspileTests
{
    [Test]
    public async Task ModuleFunction_ObjectArgs_EmitsObjectParamSignature()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile, ExportedAsModule]
            public static class UI
            {
                [ObjectArgs]
                public static int Column(int gap = 0, int width = 100) => gap + width;
            }
            """
        );

        var output = result["ui.ts"];
        await Assert
            .That(output)
            .Contains("function column(args: { gap?: number; width?: number }): number");
        await Assert.That(output).Contains("const { gap = 0, width = 100 } = args;");
    }

    [Test]
    public async Task ModuleFunction_ObjectArgs_RequiredParamHasNoQuestionMark()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile, ExportedAsModule]
            public static class UI
            {
                [ObjectArgs]
                public static int Layout(int width, int padding = 0) => width + padding;
            }
            """
        );

        var output = result["ui.ts"];
        await Assert
            .That(output)
            .Contains("function layout(args: { width: number; padding?: number }): number");
    }

    [Test]
    public async Task CallSite_ObjectArgs_PositionalAndNamedCollapseToObjectLiteral()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile, ExportedAsModule]
            public static class UI
            {
                [ObjectArgs]
                public static int Column(int width, int gap = 0) => width + gap;
            }

            [Transpile]
            public class Widget
            {
                public int A() => UI.Column(width: 100, gap: 12);
                public int B() => UI.Column(50, 4);
                public int C() => UI.Column(width: 200);
            }
            """
        );

        var output = result["widget.ts"];
        await Assert.That(output).Contains("UI.column({ width: 100, gap: 12 })");
        await Assert.That(output).Contains("UI.column({ width: 50, gap: 4 })");
        await Assert.That(output).Contains("UI.column({ width: 200 })");
    }

    [Test]
    public async Task InstanceMethod_ObjectArgs_EmitsObjectParamSignature()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public class Renderer
            {
                [ObjectArgs]
                public int Make(int gap = 0, int width = 100) => gap + width;
            }
            """
        );

        var output = result["renderer.ts"];
        await Assert.That(output).Contains("make(args: { gap?: number; width?: number }): number");
        await Assert.That(output).Contains("const { gap = 0, width = 100 } = args;");
    }

    [Test]
    public async Task InstanceMethod_ObjectArgs_RequiredParamHasNoQuestionMark()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public class Renderer
            {
                [ObjectArgs]
                public int Layout(int width, int padding = 0) => width + padding;
            }
            """
        );

        var output = result["renderer.ts"];
        await Assert
            .That(output)
            .Contains("layout(args: { width: number; padding?: number }): number");
    }

    [Test]
    public async Task InstanceMethod_ObjectArgs_CallSitesCollapseToObjectLiteral()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public class Renderer
            {
                [ObjectArgs]
                public int Make(int width, int gap = 0) => width + gap;
            }

            [Transpile]
            public class Caller
            {
                public int A(Renderer r) => r.Make(width: 100, gap: 12);
                public int B(Renderer r) => r.Make(50, 4);
                public int C(Renderer r) => r.Make(width: 200);
            }
            """
        );

        var output = result["caller.ts"];
        await Assert.That(output).Contains("r.make({ width: 100, gap: 12 })");
        await Assert.That(output).Contains("r.make({ width: 50, gap: 4 })");
        await Assert.That(output).Contains("r.make({ width: 200 })");
    }

    [Test]
    public async Task InstanceMethod_NoObjectArgs_KeepsPositionalSignature()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public class Renderer
            {
                public int Plain(int gap = 0, int width = 100) => gap + width;
            }
            """
        );

        var output = result["renderer.ts"];
        await Assert.That(output).Contains("plain(gap: number = 0, width: number = 100): number");
        await Assert.That(output).DoesNotContain("plain(args:");
    }

    [Test]
    public async Task StaticMethod_ObjectArgs_EmitsObjectParamSignature()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public class Renderer
            {
                [ObjectArgs]
                public static int Make(int gap = 0, int width = 100) => gap + width;
            }
            """
        );

        var output = result["renderer.ts"];
        await Assert
            .That(output)
            .Contains("static make(args: { gap?: number; width?: number }): number");
        await Assert.That(output).Contains("const { gap = 0, width = 100 } = args;");
    }

    [Test]
    public async Task StaticMethod_ObjectArgs_CallSiteCollapsesToObjectLiteral()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public class Renderer
            {
                [ObjectArgs]
                public static int Make(int gap = 0, int width = 100) => gap + width;
            }

            [Transpile]
            public class Caller
            {
                public int A() => Renderer.Make(gap: 12, width: 200);
            }
            """
        );

        var output = result["caller.ts"];
        await Assert.That(output).Contains("Renderer.make({ gap: 12, width: 200 })");
    }

    [Test]
    public async Task ModuleFunction_NoObjectArgs_KeepsPositionalSignature()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile, ExportedAsModule]
            public static class UI
            {
                public static int Plain(int gap = 0, int width = 100) => gap + width;
            }
            """
        );

        var output = result["ui.ts"];
        await Assert
            .That(output)
            .Contains("function plain(gap: number = 0, width: number = 100): number");
        await Assert.That(output).DoesNotContain("function plain(args:");
    }

    [Test]
    public async Task ObjectArgs_OnOverloadedMethod_EmitsDiagnosticAndSkips()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            namespace App;

            [Transpile]
            public class Renderer
            {
                [ObjectArgs]
                public int Make(int gap) => gap;
                public int Make(int gap, int width) => gap + width;
            }
            """
        );

        await Assert
            .That(
                diagnostics.Any(d =>
                    d.Code == Metano.Compiler.Diagnostics.DiagnosticCodes.UnsupportedFeature
                    && d.Message.Contains("[ObjectArgs]")
                    && d.Message.Contains("overloading")
                )
            )
            .IsTrue();
    }
}
