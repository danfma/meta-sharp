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
        await Assert.That(output).Contains("function column(args:");
        await Assert.That(output).Contains("gap?: number");
        await Assert.That(output).Contains("width?: number");
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
        await Assert.That(output).Contains("width: number");
        await Assert.That(output).Contains("padding?: number");
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
}
