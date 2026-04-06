namespace MetaSharp.Tests;

public class AssemblyTranspileTests
{
    [Test]
    public async Task TranspileAssembly_TranspilesAllPublicTypes()
    {
        var result = TranspileHelper.Transpile(
            """
            [assembly: TranspileAssembly]

            public record Point(int X, int Y);
            public enum Color { Red, Green, Blue }
            public class Service { }
            """
        );

        await Assert.That(result).ContainsKey("Point.ts");
        await Assert.That(result).ContainsKey("Color.ts");
        await Assert.That(result).ContainsKey("Service.ts");
    }

    [Test]
    public async Task TranspileAssembly_SkipsNonPublicTypes()
    {
        var result = TranspileHelper.Transpile(
            """
            [assembly: TranspileAssembly]

            public record Visible(int X);
            internal record Hidden(int Y);
            """
        );

        await Assert.That(result).ContainsKey("Visible.ts");
        await Assert.That(result).DoesNotContainKey("Hidden.ts");
    }

    [Test]
    public async Task NoTranspile_ExcludesType()
    {
        var result = TranspileHelper.Transpile(
            """
            [assembly: TranspileAssembly]

            public record Included(int X);

            [NoTranspile]
            public record Excluded(int Y);
            """
        );

        await Assert.That(result).ContainsKey("Included.ts");
        await Assert.That(result).DoesNotContainKey("Excluded.ts");
    }

    [Test]
    public async Task ExplicitTranspile_StillWorks()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public record Item(string Name);
            """
        );

        await Assert.That(result).ContainsKey("Item.ts");
    }

    [Test]
    public async Task NoTranspile_OverridesExplicitTranspile()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile, NoTranspile]
            public record Conflicted(int X);
            """
        );

        await Assert.That(result).DoesNotContainKey("Conflicted.ts");
    }

    [Test]
    public async Task AssemblyWide_InheritanceWorks()
    {
        var result = TranspileHelper.Transpile(
            """
            [assembly: TranspileAssembly]

            namespace App
            {
                public record Base(int Id);
                public record Child(int Id, string Name) : Base(Id);
            }
            """
        );

        var childTs = result["Child.ts"];
        await Assert.That(childTs).Contains("extends Base");
    }

    [Test]
    public async Task AssemblyWide_InterfaceImplementsWorks()
    {
        var result = TranspileHelper.Transpile(
            """
            [assembly: TranspileAssembly]

            namespace App
            {
                public interface IEntity { string Id { get; } }
                public record User(string Id, string Name) : IEntity;
            }
            """
        );

        var userTs = result["User.ts"];
        await Assert.That(userTs).Contains("implements IEntity");
    }

    [Test]
    public async Task AssemblyWide_WithGuardAttribute()
    {
        var result = TranspileHelper.Transpile(
            """
            [assembly: TranspileAssembly]

            [GenerateGuard]
            public record Point(int X, int Y);

            public record Line(int Length);
            """
        );

        // Point has guard (explicit [GenerateGuard])
        await Assert.That(result["Point.ts"]).Contains("isPoint");
        // Line does NOT have guard (no [GenerateGuard])
        await Assert.That(result["Line.ts"]).DoesNotContain("isLine");
    }
}
