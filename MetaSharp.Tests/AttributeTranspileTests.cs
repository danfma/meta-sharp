namespace MetaSharp.Tests;

public class AttributeTranspileTests
{
    [Test]
    public async Task UnannotatedTypes_AreNotTranspiled()
    {
        var result = TranspileHelper.Transpile("""
            public record struct Hidden(int X);

            [Transpile]
            public record struct Visible(int Y);
            """);

        await Assert.That(result).DoesNotContainKey("Hidden.ts");
        await Assert.That(result).ContainsKey("Visible.ts");
    }

    [Test]
    public async Task IgnoredMembers_AreNotTranspiled()
    {
        var result = TranspileHelper.Transpile("""
            [Transpile]
            public readonly record struct Foo(int X)
            {
                [Ignore]
                public int Secret() => 42;

                public int Visible() => X;
            }
            """);

        await Assert.That(result["Foo.ts"]).DoesNotContain("secret");
        await Assert.That(result["Foo.ts"]).Contains("visible");
    }
}
