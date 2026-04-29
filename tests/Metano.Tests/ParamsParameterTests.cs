namespace Metano.Tests;

public class ParamsParameterTests
{
    [Test]
    public async Task InstanceMethod_ParamsObjectArray_EmitsRestParameter()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public sealed class Logger
            {
                public void Log(string format, params object[] args) { }
            }
            """
        );

        var output = result["logger.ts"];
        await Assert.That(output).Contains("log(format: string, ...args: Object[])");
    }

    [Test]
    public async Task InstanceMethod_ParamsGenericArray_EmitsRestParameter()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public sealed class Bag<T>
            {
                public void AddRange(params T[] items) { }
            }
            """
        );

        var output = result["bag.ts"];
        await Assert.That(output).Contains("addRange(...items: T[])");
    }

    [Test]
    public async Task CallSite_DiscreteArgs_PassThroughWithoutArrayWrap()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public sealed class Logger
            {
                public void Log(string format, params object[] args) { }

                public void Demo()
                {
                    Log("hi {0} {1}", "alice", 42);
                }
            }
            """
        );

        var output = result["logger.ts"];
        await Assert.That(output).Contains("this.log(\"hi {0} {1}\", \"alice\", 42)");
        await Assert.That(output).DoesNotContain("[\"alice\", 42]");
    }

    [Test]
    public async Task ModuleFunction_Params_EmitsRestParameter()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            [Erasable]
            public static class Logger
            {
                public static void Log(string format, params object[] args) { }
            }
            """
        );

        var output = result["logger.ts"];
        await Assert.That(output).Contains("function log(format: string, ...args: Object[])");
    }

    [Test]
    public async Task Constructor_Params_EmitsRestParameter()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public sealed class Tagged
            {
                public Tagged(string label, params string[] tags) { }
            }
            """
        );

        var output = result["tagged.ts"];
        await Assert.That(output).Contains("constructor(label: string, ...tags: string[])");
    }

    [Test]
    public async Task Interface_Params_EmitsRestParameter()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public interface IPrinter
            {
                void Write(string format, params object[] args);
            }
            """
        );

        var output = result["i-printer.ts"];
        await Assert.That(output).Contains("write(format: string, ...args: Object[])");
    }

    [Test]
    public async Task Record_PositionalParams_DegradesToPlainArrayPropertyParam()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public sealed record Tagged(string Label, params string[] Tags);
            """
        );

        var output = result["tagged.ts"];
        await Assert.That(output).Contains("readonly tags: string[]");
        await Assert.That(output).DoesNotContain("readonly ...tags");
        await Assert.That(output).DoesNotContain("public ...tags");
    }

    [Test]
    public async Task CallSite_ExplicitArrayArgument_IsSpread()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public sealed class Logger
            {
                public void Log(string format, params string[] args) { }

                public void Demo(string[] tags)
                {
                    Log("hi", tags);
                }
            }
            """
        );

        var output = result["logger.ts"];
        await Assert.That(output).Contains("this.log(\"hi\", ...tags)");
    }

    [Test]
    public async Task NewExpression_ExplicitArrayArgument_IsSpread()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public sealed class Tagged
            {
                public Tagged(string label, params string[] tags) { }
            }

            [Transpile]
            public sealed class Caller
            {
                public Tagged Make(string[] tags) => new Tagged("hi", tags);
            }
            """
        );

        var output = result["caller.ts"];
        await Assert.That(output).Contains("new Tagged(\"hi\", ...tags)");
    }

    [Test]
    public async Task CustomDelegate_Params_EmitsRestParameter()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            public delegate void LoggerFn(string format, params object[] args);

            [Transpile]
            public sealed class Host
            {
                public LoggerFn? Logger { get; set; }
            }
            """
        );

        var output = result["host.ts"];
        await Assert.That(output).Contains("(format: string, ...args: Object[]) => void");
    }
}
