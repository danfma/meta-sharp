namespace MetaSharp.Tests;

public class InheritanceTranspileTests
{
    [Test]
    public async Task BaseRecord_GeneratesNormally()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile]
                public record Shape(int X, int Y);

                [Transpile]
                public record Circle(int X, int Y, double Radius) : Shape(X, Y);
            }
            """
        );

        var expected = TranspileHelper.ReadExpected("InheritanceBase.ts");
        await Assert.That(result["Shape.ts"]).IsEqualTo(expected);
    }

    [Test]
    public async Task DerivedRecord_ExtendsBaseWithSuper()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile]
                public record Shape(int X, int Y);

                [Transpile]
                public record Circle(int X, int Y, double Radius) : Shape(X, Y)
                {
                    public double Area() => 3.14159 * Radius * Radius;
                }
            }
            """
        );

        var expected = TranspileHelper.ReadExpected("InheritanceDerived.ts");
        await Assert.That(result["Circle.ts"]).IsEqualTo(expected);
    }

    [Test]
    public async Task DerivedRecord_HasExtendsKeyword()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile]
                public record Base(string Name);

                [Transpile]
                public record Child(string Name, int Age) : Base(Name);
            }
            """
        );

        var childTs = result["Child.ts"];
        await Assert.That(childTs).Contains("extends Base");
        await Assert.That(childTs).Contains("super(name)");
    }

    [Test]
    public async Task DerivedRecord_BaseParamsNotReadonly()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile]
                public record Base(string Name);

                [Transpile]
                public record Child(string Name, int Age) : Base(Name);
            }
            """
        );

        var childTs = result["Child.ts"];
        // Only own params in constructor (base params are declared in parent)
        await Assert.That(childTs).Contains("constructor(readonly age: number)");
        await Assert.That(childTs).Contains("super(name)");
    }

    [Test]
    public async Task DerivedRecord_EqualsIncludesAllFields()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile]
                public record Base(int X);

                [Transpile]
                public record Derived(int X, int Y) : Base(X);
            }
            """
        );

        var derivedTs = result["Derived.ts"];
        // equals should check both x and y
        await Assert.That(derivedTs).Contains("this.x === other.x");
        await Assert.That(derivedTs).Contains("this.y === other.y");
    }

    [Test]
    public async Task DerivedRecord_ImportBaseType()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile]
                public record Base(int X);
            }

            namespace App.Models
            {
                [Transpile]
                public record Extended(int X, string Label) : App.Base(X);
            }
            """
        );

        var extendedTs = result["Models/Extended.ts"];
        await Assert.That(extendedTs).Contains("from \"../Base\"");
    }

    [Test]
    public async Task NonTranspiledBase_NoExtends()
    {
        var result = TranspileHelper.Transpile(
            """
            public record NotTranspiled(int X);

            [Transpile]
            public record Child(int X, int Y) : NotTranspiled(X);
            """
        );

        var childTs = result["Child.ts"];
        // Should not extend a non-transpiled type
        await Assert.That(childTs).DoesNotContain("extends");
    }
}
