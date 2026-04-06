namespace MetaSharp.Tests;

public class EnumTranspileTests
{
    [Test]
    public async Task StringEnum_GeneratesUnionType()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile, StringEnum]
            public enum Color
            {
                [Name("RED")] Red,
                [Name("GREEN")] Green,
                [Name("BLUE")] Blue,
            }
            """
        );

        var expected = TranspileHelper.ReadExpected("StringEnum.ts");
        await Assert.That(result["Color.ts"]).IsEqualTo(expected);
    }

    [Test]
    public async Task StringEnumWithoutNameAttribute_UsesOriginalMemberName()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile, StringEnum]
            public enum Priority
            {
                Low,
                Medium,
                High,
            }
            """
        );

        var expected = TranspileHelper.ReadExpected("StringEnumNoAliases.ts");
        await Assert.That(result["Priority.ts"]).IsEqualTo(expected);
    }

    [Test]
    public async Task NumericEnum_GeneratesEnumDeclaration()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public enum Priority
            {
                Low,
                Medium,
                High,
            }
            """
        );

        var expected = TranspileHelper.ReadExpected("NumericEnum.ts");
        await Assert.That(result["Priority.ts"]).IsEqualTo(expected);
    }
}
