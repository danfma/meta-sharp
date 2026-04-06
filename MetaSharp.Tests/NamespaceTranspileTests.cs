namespace MetaSharp.Tests;

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
        await Assert.That(result).ContainsKey("Status.ts");
        await Assert.That(result).ContainsKey("User.ts");
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
        await Assert.That(result).ContainsKey("Currency.ts");
        await Assert.That(result).ContainsKey("Models/Price.ts");
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

        var priceTs = result["Models/Price.ts"];
        // Import should go up one level to find Currency
        await Assert.That(priceTs).Contains("from \"../Currency\"");
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

        // Root index should re-export Currency and sub-namespace
        await Assert.That(result).ContainsKey("index.ts");
        var rootIndex = result["index.ts"];
        await Assert.That(rootIndex).Contains("export type { Currency } from \"./Currency\"");

        // Models index should re-export Price
        await Assert.That(result).ContainsKey("Models/index.ts");
        var modelsIndex = result["Models/index.ts"];
        await Assert.That(modelsIndex).Contains("export { Price } from \"./Price\"");
    }

    [Test]
    public async Task RootIndex_ReExportsSubDirectories()
    {
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
        await Assert.That(rootIndex).Contains("export * from \"./Models\"");
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

        var moneyTs = result["Money.ts"];
        await Assert.That(moneyTs).Contains("from \"./Currency\"");
    }
}
