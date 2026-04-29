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
