namespace MetaSharp.Tests;

public class RecordTranspileTests
{
    [Test]
    public async Task SimpleRecord_GeneratesClass()
    {
        var result = TranspileHelper.Transpile("""
            [Transpile]
            public readonly record struct Point(int X, int Y);
            """);

        var expected = TranspileHelper.ReadExpected("SimpleRecord.ts");
        await Assert.That(result["Point.ts"]).IsEqualTo(expected);
    }

    [Test]
    public async Task RecordWithMethods_GeneratesClassWithMethods()
    {
        var result = TranspileHelper.Transpile("""
            [Transpile]
            public readonly record struct Amount(decimal Value)
            {
                public decimal Doubled() => Value * 2;

                public static Amount FromValue(decimal value) => new(value);
            }
            """);

        var expected = TranspileHelper.ReadExpected("RecordWithMethods.ts");
        await Assert.That(result["Amount.ts"]).IsEqualTo(expected);
    }

    [Test]
    public async Task InstanceMethod_QualifiesWithSelfParameter()
    {
        var result = TranspileHelper.Transpile("""
            [Transpile]
            public readonly record struct Amount(decimal Value)
            {
                public decimal Doubled() => Value * 2;
            }
            """);

        // Instance methods use 'this' now (class-based pattern)
        await Assert.That(result["Amount.ts"]).Contains("this.value");
    }
}
