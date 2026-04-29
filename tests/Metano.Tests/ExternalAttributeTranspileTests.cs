using Metano.Compiler.Diagnostics;

namespace Metano.Tests;

/// <summary>
/// Tests for <c>[External]</c> from
/// <c>Metano.Annotations.TypeScript</c>. Per #106 the attribute marks
/// an ambient runtime-provided shape: no file is emitted, but static
/// member access stays class-qualified at the call site
/// (<c>Js.Document</c> in C# becomes <c>Js.document</c> in TS — the
/// flatten now lives exclusively on <c>[Erasable]</c>). The widened
/// surface accepts <c>class</c>, <c>abstract class</c>, <c>interface</c>,
/// and <c>struct</c> targets so ambient structural shapes (DOM types,
/// Hono-style context interfaces) can use <c>[External]</c> directly.
/// </summary>
public class ExternalAttributeTranspileTests
{
    // ─── Emission contract — class-qualified access ─────────

    [Test]
    public async Task External_StaticProperty_AccessStaysClassQualified()
    {
        // Per #106: `[External]` no longer flattens. `Js.Document` on the
        // C# side lowers to `Js.document` on the TS side; the runtime
        // ambient declarations expose `Js` as the namespace alias around
        // the runtime global (the consumer's project provides the actual
        // `Js` object). The `[Name("document")]` on the property still
        // drives the camelCased member name.
        var result = TranspileHelper.TranspileWithLibrary(
            """
            using Metano.Annotations.TypeScript;

            [NoEmit]
            public abstract class Document {}

            [External]
            public static class Js
            {
                [Name("document")]
                public static Document Document => throw null!;
            }
            """,
            """
            [assembly: TranspileAssembly]

            public class Renderer
            {
                public Document Target => Js.Document;
            }
            """
        );

        var output = result["renderer.ts"];
        await Assert.That(output).Contains("return Js.document;");
    }

    [Test]
    public async Task External_StaticProperty_ChainedAccessKeepsRoot()
    {
        // `Js.Document.Body` lowers to `Js.document.body` — the root
        // qualifier survives because `[External]` no longer flattens.
        var result = TranspileHelper.TranspileWithLibrary(
            """
            using Metano.Annotations.TypeScript;

            [NoEmit, Name("HTMLElement")]
            public abstract class HtmlElement {}

            [NoEmit]
            public abstract class Document
            {
                public HtmlElement Body => throw null!;
            }

            [External]
            public static class Js
            {
                [Name("document")]
                public static Document Document => throw null!;
            }
            """,
            """
            [assembly: TranspileAssembly]

            public class Renderer
            {
                public HtmlElement Root => Js.Document.Body;
            }
            """
        );

        var output = result["renderer.ts"];
        await Assert.That(output).Contains("return Js.document.body;");
    }

    [Test]
    public async Task External_StaticMethod_CallStaysClassQualified()
    {
        // Static method call on an `[External]` class with a
        // `[Name("parseInt")]` override lowers to `Js.parseInt(s)` — the
        // qualifier survives.
        var result = TranspileHelper.TranspileWithLibrary(
            """
            using Metano.Annotations.TypeScript;

            [External]
            public static class Js
            {
                [Name("parseInt")]
                public static int ParseInt(string value) => throw null!;
            }
            """,
            """
            [assembly: TranspileAssembly]

            public class Parser
            {
                public int Parse(string s) => Js.ParseInt(s);
            }
            """
        );

        var output = result["parser.ts"];
        await Assert.That(output).Contains("return Js.parseInt(s);");
    }

    [Test]
    public async Task External_Class_EmitsNoFile()
    {
        // `[External]` classes are stubs — no .ts file emits for them
        // even when the class lives in a transpilable assembly.
        var result = TranspileHelper.Transpile(
            """
            using Metano.Annotations.TypeScript;
            [assembly: TranspileAssembly]

            [External]
            public static class Js
            {
                [Name("document")]
                public static object Document => throw null!;
            }

            public class Placeholder {}
            """
        );

        await Assert.That(result).DoesNotContainKey("js.ts");
        await Assert.That(result).ContainsKey("placeholder.ts");
    }

    // ─── Widened surface (PR1 of #106) ───────────────────────

    [Test]
    public async Task External_Interface_EmitsNoFile_NoDiagnostic()
    {
        // Per #106: `[External]` accepts interface targets so ambient
        // structural shapes (DOM types, Hono-style context interfaces)
        // can carry the attribute directly without a `[NoEmit]` ambient
        // workaround.
        var (result, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations.TypeScript;
            [assembly: TranspileAssembly]

            [External]
            public interface IAmbient
            {
                IAmbient Echo(string text);
            }

            public class Caller
            {
                public IAmbient Use(IAmbient c) => c.Echo("hi");
            }
            """
        );

        await Assert.That(result).DoesNotContainKey("i-ambient.ts");
        await Assert.That(result).ContainsKey("caller.ts");
        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.InvalidExternal))
            .IsFalse();
    }

    [Test]
    public async Task External_AbstractClass_EmitsNoFile_NoDiagnostic()
    {
        // Per #106: non-static abstract classes are accepted — they model
        // ambient shapes (DOM Element-style hierarchies) without
        // implementation.
        var (result, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations.TypeScript;
            [assembly: TranspileAssembly]

            [External]
            public abstract class Node
            {
                public abstract string Tag { get; }
            }

            public class Caller
            {
                public string Read(Node n) => n.Tag;
            }
            """
        );

        await Assert.That(result).DoesNotContainKey("node.ts");
        await Assert.That(result).ContainsKey("caller.ts");
        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.InvalidExternal))
            .IsFalse();
    }

    [Test]
    public async Task External_LambdaParameter_OmitsTypeAnnotation()
    {
        // An `[External]` ambient type used as a lambda parameter must
        // emit the lambda WITHOUT a parameter type annotation so
        // TypeScript infers from context (matches the legacy `[NoEmit]`
        // ambient behavior, now widened to cover `[External]`).
        var result = TranspileHelper.Transpile(
            """
            using Metano.Annotations.TypeScript;
            using System;
            [assembly: TranspileAssembly]

            [External]
            public interface ICtx
            {
                ICtx Send(string text);
            }

            [Import(name: "External", from: "external-lib")]
            public class External
            {
                public void Subscribe(Func<ICtx, ICtx> handler) => throw null!;
            }

            public class Caller
            {
                public void Wire(External e) => e.Subscribe(c => c.Send("hi"));
            }
            """
        );

        var output = result["caller.ts"];
        await Assert.That(output).Contains("e.subscribe((c) =>");
        await Assert.That(output).DoesNotContain("c: ICtx");
    }

    // ─── Diagnostics (MS0012) ────────────────────────────────

    [Test]
    public async Task External_OnConcreteNonStaticClass_EmitsMs0012()
    {
        // Per #106 the validator narrowed to "concrete instance class
        // with implementation" — a class that's neither static nor
        // abstract still raises MS0012 since `[External]` cannot honor
        // an emitted body.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations.TypeScript;

            [External]
            public class NotStatic {}
            """
        );

        var ms0012 = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.InvalidExternal);
        await Assert.That(ms0012).IsNotNull();
        await Assert.That(ms0012!.Message).Contains("static");
    }

    [Test]
    public async Task External_WithTranspile_EmitsMs0012()
    {
        // The two attributes are semantically incompatible — one asks
        // for no emission, the other asks for full emission. MS0012
        // surfaces the conflict at extraction time.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations.TypeScript;

            [Transpile, External]
            public static class Mixed
            {
                [Name("x")] public static int Value => 0;
            }
            """
        );

        var ms0012 = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.InvalidExternal);
        await Assert.That(ms0012).IsNotNull();
        await Assert.That(ms0012!.Message).Contains("Transpile");
    }

    [Test]
    public async Task External_OnValidStaticClass_EmitsNoDiagnostic()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations.TypeScript;

            [External]
            public static class Js
            {
                [Name("document")]
                public static object Document => throw null!;
            }
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.InvalidExternal))
            .IsFalse();
    }

    // ─── Erasable retains the flatten behavior ───────────────

    [Test]
    public async Task Erasable_StaticMethod_CallFlattens()
    {
        // Counterpart to External_StaticMethod_CallStaysClassQualified.
        // After #106, only `[Erasable]` flattens at the call site.
        var result = TranspileHelper.Transpile(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [Erasable]
            public static class Constants
            {
                public static int Pi => 3;
            }

            public class Consumer
            {
                public int Get() => Constants.Pi;
            }
            """
        );

        var output = result["consumer.ts"];
        await Assert.That(output).Contains("return pi;");
        await Assert.That(output).DoesNotContain("Constants.");
    }

    // ─── Erasable flattens vs plain static class ───────────

    [Test]
    public async Task ErasableStaticClass_AccessFlattens()
    {
        // Baseline counterpart to External_StaticMethod_CallStaysClassQualified:
        // `[Erasable]` is the lone flatten anchor after #106. A consumer
        // calling `Helpers.Zero()` lowers to a bare `zero()` because the
        // class scope vanishes at the call site.
        var result = TranspileHelper.Transpile(
            """
            [assembly: TranspileAssembly]

            [Transpile, Erasable]
            public static class Helpers
            {
                public static int Zero() => 0;
            }

            public class Consumer
            {
                public int Call() => Helpers.Zero();
            }
            """
        );

        var output = result["consumer.ts"];
        await Assert.That(output).Contains("return zero();");
        await Assert.That(output).DoesNotContain("Helpers.");
    }
}
