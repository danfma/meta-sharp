namespace MetaSharp.Tests;

public class ExpressionTranspileTests
{
    [Test]
    public async Task WithExpression_GeneratesSpread()
    {
        var result = TranspileHelper.Transpile("""
            [Transpile]
            public readonly record struct Coord(int X, int Y)
            {
                public static Coord MoveX(Coord coord, int dx) =>
                    coord with { X = coord.X + dx };
            }
            """);

        var expected = TranspileHelper.ReadExpected("WithExpression.ts");
        await Assert.That(result["Coord.ts"]).IsEqualTo(expected);
    }

    [Test]
    public async Task IfElseThrow_GeneratesCorrectStatements()
    {
        var result = TranspileHelper.Transpile("""
            [Transpile]
            public readonly record struct Pair(int A, int B)
            {
                public static void Check(Pair pair)
                {
                    if (pair.A != pair.B)
                        throw new System.Exception("mismatch");
                }
            }
            """);

        var expected = TranspileHelper.ReadExpected("IfElseThrow.ts");
        await Assert.That(result["Pair.ts"]).IsEqualTo(expected);
    }
}
