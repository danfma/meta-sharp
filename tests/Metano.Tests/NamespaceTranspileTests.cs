namespace Metano.Tests;

public class NamespaceTranspileTests
{
    [Test]
    public async Task SameNamespace_GeneratesFlatFiles()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App.Domain
            {
                [Transpile, StringEnum]
                public enum Status { Active, Inactive }

                [Transpile]
                public readonly record struct User(string Name, Status Status);
            }
            """
        );

        // Root namespace is App.Domain → both at root
        await Assert.That(result).ContainsKey("status.ts");
        await Assert.That(result).ContainsKey("user.ts");
    }

    [Test]
    public async Task DifferentNamespaces_GenerateSubFolders()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App.Domain
            {
                [Transpile, StringEnum]
                public enum Currency { Brl, Usd }
            }

            namespace App.Domain.Models
            {
                [Transpile]
                public readonly record struct Price(int Amount);
            }
            """
        );

        // Root namespace is App.Domain
        // Currency → root, Price → Models/
        await Assert.That(result).ContainsKey("currency.ts");
        await Assert.That(result).ContainsKey("models/price.ts");
    }

    [Test]
    public async Task CrossNamespaceImport_UsesRelativePath()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App.Domain
            {
                [Transpile, StringEnum]
                public enum Currency { Brl, Usd }
            }

            namespace App.Domain.Models
            {
                [Transpile]
                public readonly record struct Price(int Amount, App.Domain.Currency Currency);
            }
            """
        );

        var priceTs = result["models/price.ts"];
        // Different namespace → import the root barrel of the package/project.
        await Assert.That(priceTs).Contains("from \"#\"");
    }

    [Test]
    public async Task IndexFile_GeneratedPerDirectory()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App.Domain
            {
                [Transpile, StringEnum]
                public enum Currency { Brl, Usd }
            }

            namespace App.Domain.Models
            {
                [Transpile]
                public readonly record struct Price(int Amount);
            }
            """
        );

        // Root index should re-export Currency (StringEnum is a value, not type-only)
        await Assert.That(result).ContainsKey("index.ts");
        var rootIndex = result["index.ts"];
        await Assert.That(rootIndex).Contains("export { Currency } from \"./currency\"");

        // Models index should re-export Price
        await Assert.That(result).ContainsKey("models/index.ts");
        var modelsIndex = result["models/index.ts"];
        await Assert.That(modelsIndex).Contains("export { Price } from \"./price\"");
    }

    [Test]
    public async Task RootIndex_DoesNotReExportSubDirectories()
    {
        // Leaf-only barrels: parent index does NOT re-export subdirectories.
        var result = TranspileHelper.Transpile(
            """
            namespace App.Domain
            {
                [Transpile, StringEnum]
                public enum Status { Active }
            }

            namespace App.Domain.Models
            {
                [Transpile]
                public readonly record struct Item(string Name);
            }
            """
        );

        var rootIndex = result["index.ts"];
        await Assert.That(rootIndex).DoesNotContain("export * from \"./models\"");
    }

    [Test]
    public async Task SameNamespaceImport_UsesCurrentDirectory()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App.Domain
            {
                [Transpile, StringEnum]
                public enum Currency { Brl }

                [Transpile]
                public readonly record struct Money(int Cents, Currency Currency);
            }
            """
        );

        var moneyTs = result["money.ts"];
        // Same namespace uses relative file import to avoid a cycle through
        // the namespace barrel (`money.ts -> barrel -> ./money.ts`).
        await Assert.That(moneyTs).Contains("from \"./currency\"");
    }

    [Test]
    public async Task MultipleTypesFromSameBarrel_MergeIntoSingleImportLine()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App.Domain
            {
                [Transpile]
                public class Priority { }

                [Transpile]
                public class Status { }

                [Transpile]
                public class Category { }
            }

            namespace App.Application
            {
                [Transpile]
                public sealed class Ticket
                {
                    public Ticket()
                    {
                        Source = new App.Domain.Priority();
                        Current = new App.Domain.Status();
                        Kind = new App.Domain.Category();
                    }

                    public App.Domain.Priority Source { get; }
                    public App.Domain.Status Current { get; }
                    public App.Domain.Category Kind { get; }
                }
            }
            """
        );

        var ticketTs = result["application/ticket.ts"];
        await Assert
            .That(ticketTs)
            .Contains("import { Category, Priority, Status } from \"#/domain\";");
        await Assert.That(ticketTs).DoesNotContain("import { Category } from \"#/domain\"");
        await Assert.That(ticketTs).DoesNotContain("import { Priority } from \"#/domain\"");
        await Assert.That(ticketTs).DoesNotContain("import { Status } from \"#/domain\"");
    }

    [Test]
    public async Task AllTypeOnlyFromSameBarrel_UsesWholeStatementImportType()
    {
        // When every name in a bucket is type-only, prefer the whole-statement
        // `import type { … }` form over per-name `{ type A, type B }` — the
        // latter triggers Biome's noImportTypeQualifier warning.
        var result = TranspileHelper.Transpile(
            """
            namespace App.Domain
            {
                [Transpile]
                public interface IReadable { string Read(); }

                [Transpile]
                public interface IWritable { void Write(string value); }
            }

            namespace App.Application
            {
                [Transpile]
                public interface IHandler
                {
                    void Handle(App.Domain.IReadable reader, App.Domain.IWritable writer);
                }
            }
            """
        );

        var handlerTs = result["application/i-handler.ts"];
        await Assert
            .That(handlerTs)
            .Contains("import type { IReadable, IWritable } from \"#/domain\";");
        // No per-name type qualifier form.
        await Assert.That(handlerTs).DoesNotContain("{ type IReadable");
        await Assert.That(handlerTs).DoesNotContain("type IWritable }");
    }

    [Test]
    public async Task MixedValueAndTypeOnlyFromSameBarrel_UsesPerNameQualifier()
    {
        // Mixed bucket: values stay plain, types get the inline `type` qualifier.
        var result = TranspileHelper.Transpile(
            """
            namespace App.Domain
            {
                [Transpile, StringEnum]
                public enum Priority { Low, High }

                [Transpile]
                public interface IReadable { string Read(); }
            }

            namespace App.Application
            {
                [Transpile]
                public sealed class Job
                {
                    public Job(App.Domain.IReadable reader)
                    {
                        Reader = reader;
                        Current = App.Domain.Priority.Low;
                    }

                    public App.Domain.Priority Current { get; }
                    public App.Domain.IReadable Reader { get; }
                }
            }
            """
        );

        var jobTs = result["application/job.ts"];
        await Assert.That(jobTs).Contains("import { Priority, type IReadable } from \"#/domain\";");
    }
}
