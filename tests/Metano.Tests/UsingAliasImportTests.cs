namespace Metano.Tests;

public class UsingAliasImportTests
{
    [Test]
    public async Task UsingAlias_AppliesToReturnTypeAndImport()
    {
        var result = TranspileHelper.Transpile(
            """
            using Metano.Annotations;
            using ColumnWidget = App.Widgets.Column;

            namespace App.Widgets
            {
                [Transpile]
                public class Column
                {
                    public int Gap { get; }
                    public Column(int gap) { Gap = gap; }
                }
            }

            namespace App
            {
                [Transpile]
                public class Ui
                {
                    public ColumnWidget Make() => new ColumnWidget(12);
                }
            }
            """
        );

        var output = result["ui.ts"];
        await Assert.That(output).Contains("import { Column as ColumnWidget }");
        await Assert.That(output).Contains("new ColumnWidget(12)");
    }

    [Test]
    public async Task UsingAlias_ResolvesNameClashWithLocalFactory()
    {
        var result = TranspileHelper.Transpile(
            """
            using Metano.Annotations;
            using ColumnWidget = App.Widgets.Column;

            namespace App.Widgets
            {
                [Transpile]
                public class Column
                {
                    public int Gap { get; }
                    public Column(int gap) { Gap = gap; }
                }
            }

            namespace App
            {
                [Transpile, ExportedAsModule]
                public static class Ui
                {
                    [Name(TargetLanguage.TypeScript, "Column")]
                    public static ColumnWidget Column(int gap) => new ColumnWidget(gap);
                }
            }
            """
        );

        var output = result["ui.ts"];
        await Assert.That(output).Contains("import { Column as ColumnWidget }");
        await Assert.That(output).Contains("export function Column(gap: number): ColumnWidget");
        await Assert.That(output).Contains("return new ColumnWidget(gap);");
    }

    [Test]
    public async Task ErasableFactoryNameClash_AutoAliasesAndEmitsInfo()
    {
        var (result, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;

            namespace App.Widgets
            {
                [Transpile]
                public class Column
                {
                    public int Gap { get; }
                    public Column(int gap) { Gap = gap; }
                }
            }

            namespace App
            {
                [Transpile, Erasable]
                public static class Ui
                {
                    [Name(TargetLanguage.TypeScript, "Column")]
                    public static App.Widgets.Column Column(int gap) => new App.Widgets.Column(gap);
                }
            }
            """
        );

        var output = result["ui.ts"];
        await Assert.That(output).Contains("import { Column as ColumnFromWidgets }");
        await Assert.That(output).Contains("export function Column(gap: number): ColumnFromWidgets");
        await Assert.That(output).Contains("return new ColumnFromWidgets(gap);");

        var info = diagnostics.FirstOrDefault(d =>
            d.Code == Metano.Compiler.Diagnostics.DiagnosticCodes.AliasedImportConflict
        );
        await Assert.That(info).IsNotNull();
        await Assert.That(info!.Severity).IsEqualTo(Metano.Compiler.Diagnostics.MetanoDiagnosticSeverity.Info);
        await Assert.That(info.Message).Contains("ColumnFromWidgets");
    }

    [Test]
    public async Task UsingAlias_OverridesAutoSynthesis()
    {
        var (result, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            using ColumnWidget = App.Widgets.Column;

            namespace App.Widgets
            {
                [Transpile]
                public class Column
                {
                    public int Gap { get; }
                    public Column(int gap) { Gap = gap; }
                }
            }

            namespace App
            {
                [Transpile, Erasable]
                public static class Ui
                {
                    [Name(TargetLanguage.TypeScript, "Column")]
                    public static ColumnWidget Column(int gap) => new ColumnWidget(gap);
                }
            }
            """
        );

        var output = result["ui.ts"];
        await Assert.That(output).Contains("import { Column as ColumnWidget }");
        await Assert.That(output).DoesNotContain("ColumnFromWidgets");

        await Assert
            .That(
                diagnostics.Any(d =>
                    d.Code == Metano.Compiler.Diagnostics.DiagnosticCodes.AliasedImportConflict
                )
            )
            .IsFalse();
    }

    [Test]
    public async Task NoUsingAlias_KeepsCanonicalImport()
    {
        var result = TranspileHelper.Transpile(
            """
            using Metano.Annotations;

            namespace App.Widgets
            {
                [Transpile]
                public class Column
                {
                    public int Gap { get; }
                    public Column(int gap) { Gap = gap; }
                }
            }

            namespace App
            {
                [Transpile]
                public class Caller
                {
                    public App.Widgets.Column Make() => new App.Widgets.Column(12);
                }
            }
            """
        );

        var output = result["caller.ts"];
        await Assert.That(output).Contains("import { Column }");
        await Assert.That(output).DoesNotContain(" as ");
        await Assert.That(output).Contains("new Column(12)");
    }
}
