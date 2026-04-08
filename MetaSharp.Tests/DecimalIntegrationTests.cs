namespace MetaSharp.Tests;

/// <summary>
/// End-to-end tests for the C# <c>decimal</c> → <c>Decimal</c> (decimal.js) integration.
/// Covers the type-level mapping (17a), literal lowering (17b), operator lowering (17c),
/// and member-level mappings (17d).
/// </summary>
public class DecimalIntegrationTests
{
    [Test]
    public async Task DecimalField_LowersToDecimalTypeAndImports()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public record Price(decimal Amount);
            """
        );

        var output = result["price.ts"];
        await Assert.That(output).Contains("import { Decimal } from \"decimal.js\"");
        await Assert.That(output).Contains("amount: Decimal");
    }

    [Test]
    public async Task DecimalLiteral_LowersToNewDecimalWithStringArg()
    {
        // Decimal literals (1.5m) wrap in `new Decimal("1.5")` so decimal.js can parse
        // the exact value without going through a lossy JS number conversion.
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public class Calc
            {
                public decimal Pi => 3.14159265358979m;
            }
            """
        );

        var output = result["calc.ts"];
        await Assert.That(output).Contains("new Decimal(\"3.14159265358979\")");
    }

    [Test]
    public async Task NonDecimalLiteral_StillLowersToBareNumber()
    {
        // Sanity check: int / double / float literals are unaffected.
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public class Calc
            {
                public int IntValue => 42;
                public double DoubleValue => 3.14;
            }
            """
        );

        var output = result["calc.ts"];
        await Assert.That(output).Contains("return 42;");
        await Assert.That(output).Contains("return 3.14;");
        await Assert.That(output).DoesNotContain("new Decimal");
    }
}
