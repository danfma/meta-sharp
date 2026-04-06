namespace MetaSharp.Tests;

public class GenericTranspileTests
{
    [Test]
    public async Task GenericRecord_EmitsTypeParameter()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public record Result<T>(T Value, bool Success);
            """
        );

        var expected = TranspileHelper.ReadExpected("GenericRecord.ts");
        await Assert.That(result["Result.ts"]).IsEqualTo(expected);
    }

    [Test]
    public async Task GenericRecord_MultipleTypeParams()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public record Pair<K, V>(K Key, V Value);
            """
        );

        var expected = TranspileHelper.ReadExpected("GenericMultiParam.ts");
        await Assert.That(result["Pair.ts"]).IsEqualTo(expected);
    }

    [Test]
    public async Task GenericRecord_WithConstraint()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile]
                public interface IEntity { string Id { get; } }

                [Transpile]
                public record Repo<T>(T Item) where T : IEntity;
            }
            """
        );

        var expected = TranspileHelper.ReadExpected("GenericConstraint.ts");
        await Assert.That(result["Repo.ts"]).IsEqualTo(expected);
    }

    [Test]
    public async Task GenericInterface_EmitsTypeParameter()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public interface IRepository<T>
            {
                System.Collections.Generic.IReadOnlyList<T> Items { get; }
            }
            """
        );

        var expected = TranspileHelper.ReadExpected("GenericInterface.ts");
        await Assert.That(result["IRepository.ts"]).IsEqualTo(expected);
    }

    [Test]
    public async Task GenericRecord_Inheritance()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile]
                public record Result<T>(T Value, bool Success);

                [Transpile]
                public record Ok<T>(T Value) : Result<T>(Value, true);
            }
            """
        );

        var okTs = result["Ok.ts"];
        await Assert.That(okTs).Contains("class Ok<T> extends Result<T>");
        await Assert.That(okTs).Contains("super(value, true)");
    }

    [Test]
    public async Task GenericMethod_EmitsTypeParameter()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile, ExportedAsModule]
            public static class Parser
            {
                public static T Identity<T>(T value) => value;
            }
            """
        );

        var output = result["Parser.ts"];
        await Assert.That(output).Contains("function identity<T>(value: T): T");
    }

    [Test]
    public async Task GenericMethod_WithConstraint()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile]
                public interface IEntity { string Id { get; } }

                [Transpile, ExportedAsModule]
                public static class Finder
                {
                    public static T Find<T>(T[] items, string id) where T : IEntity
                    {
                        return items[0];
                    }
                }
            }
            """
        );

        var output = result["Finder.ts"];
        await Assert.That(output).Contains("function find<T extends IEntity>(items: T[], id: string): T");
    }

    [Test]
    public async Task ConcreteGenericType_PreservesTypeArguments()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public record Wrapper(System.Collections.Generic.List<int> Numbers);
            """
        );

        var output = result["Wrapper.ts"];
        // List<int> → number[] (already handled by TypeMapper)
        await Assert.That(output).Contains("numbers: number[]");
    }

    [Test]
    public async Task PartialInWith_IsStructural()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public record Box<T>(T Content);
            """
        );

        var output = result["Box.ts"];
        // Partial<Box<T>> should be structural, not a string hack
        await Assert.That(output).Contains("Partial<Box<T>>");
        await Assert.That(output).Contains("Box<T>");
    }

    [Test]
    public async Task GenericRecord_ImplementsGenericInterface()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App
            {
                [Transpile]
                public interface IContainer<T> { T Value { get; } }

                [Transpile]
                public record Box<T>(T Value) : IContainer<T>;
            }
            """
        );

        var boxTs = result["Box.ts"];
        await Assert.That(boxTs).Contains("implements IContainer<T>");
    }
}
