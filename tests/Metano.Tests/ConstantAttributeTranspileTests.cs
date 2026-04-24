using Metano.Compiler.Diagnostics;

namespace Metano.Tests;

/// <summary>
/// Tests for <c>[Constant]</c> from <c>Metano.Annotations</c>. The
/// attribute enforces that decorated parameters receive compile-time
/// constant arguments and decorated fields are initialized with
/// compile-time constants. Violations surface as MS0014
/// InvalidConstant. Covers both validation surfaces plus the happy
/// paths (literal, `const` local/field, `readonly` constant-reducible
/// field).
/// </summary>
public class ConstantAttributeTranspileTests
{
    // ─── Parameter validator — positive paths ────────────────

    [Test]
    public async Task Constant_Parameter_LiteralArgument_Passes()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public static class Runtime
            {
                public static void Use([Constant] string tag) {}
            }

            public class Caller
            {
                public void Run() => Runtime.Use("div");
            }
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.InvalidConstant))
            .IsFalse();
    }

    [Test]
    public async Task Constant_Parameter_ConstField_Passes()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public static class Runtime
            {
                public static void Use([Constant] string tag) {}
            }

            public class Caller
            {
                private const string Tag = "div";
                public void Run() => Runtime.Use(Tag);
            }
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.InvalidConstant))
            .IsFalse();
    }

    // ─── Parameter validator — negative paths ────────────────

    [Test]
    public async Task Constant_Parameter_Variable_EmitsMs0014()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public static class Runtime
            {
                public static void Use([Constant] string tag) {}
            }

            public class Caller
            {
                public void Run(string tag) => Runtime.Use(tag);
            }
            """
        );

        var ms0014 = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.InvalidConstant);
        await Assert.That(ms0014).IsNotNull();
        await Assert.That(ms0014!.Message).Contains("'tag'");
    }

    [Test]
    public async Task Constant_Parameter_MethodCall_EmitsMs0014()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public static class Runtime
            {
                public static void Use([Constant] string tag) {}
            }

            public class Caller
            {
                public string Compute() => "x";
                public void Run() => Runtime.Use(Compute());
            }
            """
        );

        var ms0014 = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.InvalidConstant);
        await Assert.That(ms0014).IsNotNull();
    }

    // ─── Parameter validator — named + positional ────────────

    [Test]
    public async Task Constant_Parameter_NamedArgument_ResolvesCorrectParameter()
    {
        // When the caller uses a named argument, the validator must
        // resolve the parameter by name rather than position so the
        // `[Constant]` contract is enforced on the right arg.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public static class Runtime
            {
                public static void Use(string loose, [Constant] string tag) {}
            }

            public class Caller
            {
                public void Run(string runtimeValue) =>
                    Runtime.Use(loose: runtimeValue, tag: "div");
            }
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.InvalidConstant))
            .IsFalse();
    }

    // ─── Field validator ─────────────────────────────────────

    [Test]
    public async Task Constant_Field_LiteralInitializer_Passes()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public class Tags
            {
                [Constant] public static readonly string Div = "div";
            }
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.InvalidConstant))
            .IsFalse();
    }

    [Test]
    public async Task Constant_Field_NonConstantInitializer_EmitsMs0014()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public class Tags
            {
                public static string Compute() => "div";
                [Constant] public static readonly string Div = Compute();
            }
            """
        );

        var ms0014 = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.InvalidConstant);
        await Assert.That(ms0014).IsNotNull();
        await Assert.That(ms0014!.Message).Contains("Div");
    }

    // ─── Constructor call site ───────────────────────────────

    [Test]
    public async Task Constant_ConstructorParameter_LiteralArgument_Passes()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public readonly record struct Tag([Constant] string Value);

            public class Caller
            {
                public Tag Make() => new Tag("div");
            }
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.InvalidConstant))
            .IsFalse();
    }

    [Test]
    public async Task Constant_ConstructorParameter_Variable_EmitsMs0014()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public readonly record struct Tag([Constant] string Value);

            public class Caller
            {
                public Tag Make(string runtimeValue) => new Tag(runtimeValue);
            }
            """
        );

        var ms0014 = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.InvalidConstant);
        await Assert.That(ms0014).IsNotNull();
    }

    // ─── Field chain-of-trust ─────────────────────────────────

    [Test]
    public async Task Constant_Parameter_ConstantReadonlyField_Passes()
    {
        // A reference to a `[Constant]`-decorated readonly field
        // with a literal initializer is accepted as a constant arg
        // (chain of trust — the field's own validation already ran).
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public static class Tags
            {
                [Constant] public static readonly string Div = "div";
            }

            public static class Runtime
            {
                public static void Use([Constant] string tag) {}
            }

            public class Caller
            {
                public void Run() => Runtime.Use(Tags.Div);
            }
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.InvalidConstant))
            .IsFalse();
    }

    [Test]
    public async Task Constant_Parameter_UndecoratedReadonlyField_EmitsMs0014()
    {
        // A plain `readonly` field (no `[Constant]` on the source)
        // is rejected even when its initializer is a literal —
        // Roslyn does not fold the reference and the transpiler
        // refuses to chase the initializer without the explicit
        // attribute on the source field.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public static class Tags
            {
                public static readonly string Div = "div";
            }

            public static class Runtime
            {
                public static void Use([Constant] string tag) {}
            }

            public class Caller
            {
                public void Run() => Runtime.Use(Tags.Div);
            }
            """
        );

        var ms0014 = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.InvalidConstant);
        await Assert.That(ms0014).IsNotNull();
    }

    // ─── Field mutability ─────────────────────────────────────

    [Test]
    public async Task Constant_Field_MutableField_EmitsMs0014()
    {
        // A mutable field cannot carry `[Constant]` — the value
        // might be reassigned later and downstream lowering cannot
        // trust the compile-time reading. Even with a literal
        // initializer the field fails MS0014.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public static class Tags
            {
                [Constant] public static string Div = "div";
            }
            """
        );

        var ms0014 = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.InvalidConstant);
        await Assert.That(ms0014).IsNotNull();
        await Assert.That(ms0014!.Message).Contains("Div");
    }

    // ─── Constructor initializer walk ─────────────────────────

    [Test]
    public async Task Constant_ConstructorInitializer_ThisCall_LiteralArgument_Passes()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public class Caller
            {
                public Caller([Constant] string tag) {}
                public Caller() : this("div") {}
            }
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.InvalidConstant))
            .IsFalse();
    }

    [Test]
    public async Task Constant_ConstructorInitializer_ThisCall_Variable_EmitsMs0014()
    {
        // `: this(...)` chaining with a non-constant arg must still
        // surface MS0014 — the walk covers ConstructorInitializerSyntax
        // alongside invocation/object-creation nodes.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public class Caller
            {
                public Caller([Constant] string tag) {}
                public Caller(int _, string runtimeValue) : this(runtimeValue) {}
            }
            """
        );

        var ms0014 = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.InvalidConstant);
        await Assert.That(ms0014).IsNotNull();
    }
}
