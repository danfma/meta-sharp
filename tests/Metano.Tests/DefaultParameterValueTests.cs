namespace Metano.Tests;

/// <summary>
/// Default-value emission for method, function, and lambda parameters.
/// The IR captures the default expression on every parameter site;
/// these tests pin the TypeScript surface so callers see the same
/// optional ergonomics the C# source intends.
/// </summary>
public class DefaultParameterValueTests
{
    [Test]
    public async Task InstanceMethod_StringDefault_EmitsAssignment()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public sealed class Greeter
            {
                public string Greet(string name, string greeting = "hello") =>
                    $"{greeting}, {name}";
            }
            """
        );

        var output = result["greeter.ts"];
        await Assert.That(output).Contains("greet(name: string, greeting: string = \"hello\")");
    }

    [Test]
    public async Task ModuleFunction_BoolDefault_EmitsAssignment()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            [ExportedAsModule]
            public static class Builder
            {
                public static string Create(string tag, bool attached = true) =>
                    attached ? tag : $"<{tag}>";
            }
            """
        );

        var output = result["builder.ts"];
        await Assert
            .That(output)
            .Contains("function create(tag: string, attached: boolean = true): string");
    }

    [Test]
    public async Task NumericDefault_EmitsLiteral()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public sealed class Counter
            {
                public int Tick(int step = 1) => step;
            }
            """
        );

        var output = result["counter.ts"];
        await Assert.That(output).Contains("tick(step: number = 1)");
    }

    [Test]
    public async Task NullDefault_EmitsNullLiteral()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public sealed class Logger
            {
                public void Log(string message, string? prefix = null) { }
            }
            """
        );

        var output = result["logger.ts"];
        await Assert.That(output).Contains("log(message: string, prefix: string | null = null)");
    }

    [Test]
    public async Task NoDefault_KeepsRequiredParameter()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public sealed class Plain
            {
                public string Echo(string value) => value;
            }
            """
        );

        var output = result["plain.ts"];
        await Assert.That(output).Contains("echo(value: string): string");
        await Assert.That(output).DoesNotContain("=");
    }
}
