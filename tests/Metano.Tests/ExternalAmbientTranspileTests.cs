namespace Metano.Tests;

/// <summary>
/// Tests for the ambient contract carried by <c>[External]</c>:
/// <list type="bullet">
///   <item>Type with <c>[External]</c> generates NO .ts file</item>
///   <item>Other transpiled code can reference it (compiles in C#) but its name does
///   NOT appear as an import anywhere</item>
///   <item>When a lambda parameter's type is <c>[External]</c>, the lambda is emitted
///   without a parameter type annotation, letting TypeScript infer from context</item>
/// </list>
/// </summary>
public class ExternalAmbientTranspileTests
{
    [Test]
    public async Task ExternalType_DoesNotProduceFile()
    {
        var result = TranspileHelper.Transpile(
            """
            using Metano.Annotations.TypeScript;

            [External]
            public interface IAmbient
            {
                IAmbient Text(string text);
            }

            [Transpile]
            public class Holder
            {
                public int Value { get; set; }
            }
            """
        );

        await Assert.That(result).DoesNotContainKey("i-ambient.ts");
        await Assert.That(result).ContainsKey("holder.ts");
    }

    [Test]
    public async Task ExternalType_NotImportedFromConsumer()
    {
        // Even when a transpiled type references an [Import]'d external class whose
        // method takes a callback over an [External] interface, the .ts output must
        // not try to import the [External] type from anywhere.
        var result = TranspileHelper.Transpile(
            """
            using System;
            using Metano.Annotations.TypeScript;

            [assembly: TranspileAssembly]

            [Import(name: "ExternalLib", from: "external-lib")]
            public class ExternalLib
            {
                public void Subscribe(Action<IExternalContext> handler) => throw new NotSupportedException();
            }

            [External]
            public interface IExternalContext
            {
                IExternalContext Send(string text);
            }

            public class Wiring
            {
                public void Setup()
                {
                    var ext = new ExternalLib();
                    ext.Subscribe(c => c.Send("hi"));
                }
            }
            """
        );

        var output = result["wiring.ts"];
        await Assert.That(output).Contains("import { ExternalLib } from \"external-lib\"");
        await Assert.That(output).DoesNotContain("IExternalContext");
    }

    [Test]
    public async Task ExternalLambdaParameter_OmitsTypeAnnotation()
    {
        // The lambda parameter c has C# type IExternalContext (which is [External]).
        // The generated arrow function should have no `: IExternalContext` annotation —
        // so TypeScript infers the type from the External.subscribe signature in the
        // real .d.ts of "external-lib".
        var result = TranspileHelper.Transpile(
            """
            using System;
            using Metano.Annotations.TypeScript;

            [assembly: TranspileAssembly]

            [Import(name: "ExternalLib", from: "external-lib")]
            public class ExternalLib
            {
                public void Subscribe(Action<IExternalContext> handler) => throw new NotSupportedException();
            }

            [External]
            public interface IExternalContext
            {
                IExternalContext Send(string text);
            }

            public class Wiring
            {
                public void Setup()
                {
                    var ext = new ExternalLib();
                    ext.Subscribe(c => c.Send("hi"));
                }
            }
            """
        );

        var output = result["wiring.ts"];
        await Assert.That(output).Contains("(c) => c.send(\"hi\")");
        await Assert.That(output).DoesNotContain("c: I");
    }

    [Test]
    public async Task ExternalType_BindingLib_WithoutTranspileAssembly_SurfacesNameOverride()
    {
        // DOM binding pattern: the binding library is a plain-C# project
        // with no `[assembly: TranspileAssembly]` and no
        // `[assembly: EmitPackage]`. Every type is individually marked
        // `[External, Name("…")]` so Metano knows about them via Roslyn
        // metadata alone. The untargeted `[Name]` override must still
        // surface at reference sites on the consumer side.
        var result = TranspileHelper.TranspileWithLibrary(
            """
            using Metano.Annotations;
            using Metano.Annotations.TypeScript;

            [External, Name("HTMLElement")]
            public abstract class HtmlElement {}
            """,
            """
            [assembly: TranspileAssembly]

            public class Renderer
            {
                public HtmlElement? Target { get; set; }
            }
            """
        );

        var output = result["renderer.ts"];
        await Assert.That(output).Contains("HTMLElement");
        await Assert.That(output).DoesNotContain("HtmlElement");
    }

    [Test]
    public async Task ExternalType_CrossAssembly_SurfacesUntargetedNameOverride()
    {
        var result = TranspileHelper.TranspileWithLibrary(
            """
            using Metano.Annotations;
            using Metano.Annotations.TypeScript;

            [assembly: TranspileAssembly]
            [assembly: EmitPackage("dom-bindings")]

            [External, Name("HTMLElement")]
            public abstract class HtmlElement {}
            """,
            """
            [assembly: TranspileAssembly]

            public class Renderer
            {
                public HtmlElement? Target { get; set; }

                public void Attach(HtmlElement element) {}
            }
            """
        );

        var output = result["renderer.ts"];
        await Assert.That(output).Contains("HTMLElement");
        await Assert.That(output).DoesNotContain("HtmlElement");
    }

    [Test]
    public async Task ExternalType_WithUntargetedNameOverride_SurfacesOverrideAtReferenceSites()
    {
        var result = TranspileHelper.Transpile(
            """
            using Metano.Annotations;
            using Metano.Annotations.TypeScript;

            [assembly: TranspileAssembly]

            [External, Name("HTMLElement")]
            public abstract class HtmlElement {}

            public class Renderer
            {
                public HtmlElement? Target { get; set; }

                public void Attach(HtmlElement element) {}
            }
            """
        );

        var output = result["renderer.ts"];
        await Assert.That(output).Contains("HTMLElement");
        await Assert.That(output).DoesNotContain("HtmlElement");
    }
}
