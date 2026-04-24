using Metano.Compiler.Diagnostics;

namespace Metano.Tests;

/// <summary>
/// Tests for <c>[Inline]</c> from <c>Metano.Annotations</c>. The
/// attribute expands a member access into the member's initializer
/// (or expression-bodied getter) at every call site, so the
/// declaration itself never materializes in the generated output.
/// Combines with <c>[Erasable]</c> on the container and
/// <c>[PlainObject]</c>/<c>[Branded]</c> on the initializer type to
/// replicate TypeScript's literal-type dispatch without a helper
/// indirection.
/// </summary>
public class InlineAttributeTranspileTests
{
    [Test]
    public async Task Inline_Property_LiteralInitializer_InlinesAtAccessSite()
    {
        var result = TranspileHelper.Transpile(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [Erasable]
            public static class Constants
            {
                [Inline]
                public static string Pi => "pi";
            }

            public class Circle
            {
                public string GetPi() => Constants.Pi;
            }
            """
        );

        var output = result["circle.ts"];
        await Assert.That(output).Contains("return \"pi\";");
        await Assert.That(output).DoesNotContain("Constants.");
        await Assert.That(output).DoesNotContain("pi()");
    }

    [Test]
    public async Task Inline_Field_LiteralInitializer_InlinesAtAccessSite()
    {
        var result = TranspileHelper.Transpile(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [Erasable]
            public static class Constants
            {
                [Inline]
                public static readonly int Answer = 42;
            }

            public class Asker
            {
                public int Ask() => Constants.Answer;
            }
            """
        );

        var output = result["asker.ts"];
        await Assert.That(output).Contains("return 42;");
        await Assert.That(output).DoesNotContain("Constants.");
    }

    [Test]
    public async Task Inline_Property_PlainObjectInitializer_InlinesAsLiteral()
    {
        // The DOM-binding shape: catalog entry built from a
        // [PlainObject] record lowers to the object literal at the
        // call site, with no intermediate helper indirection.
        var result = TranspileHelper.Transpile(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [PlainObject]
            public sealed record Tag(string TagName);

            [Erasable]
            public static class HtmlElementType
            {
                [Inline]
                public static Tag Div => new("div");
            }

            public class Runtime
            {
                public Tag Describe() => HtmlElementType.Div;
            }
            """
        );

        var output = result["runtime.ts"];
        await Assert.That(output).Contains("tagName: \"div\"");
        await Assert.That(output).DoesNotContain("HtmlElementType.");
    }

    // ─── Cross-assembly ───────────────────────────────────────

    [Test]
    public async Task Inline_CrossAssembly_ExpandsFromProjectReference()
    {
        // Regression for issue #109: an [Inline] member declared in a
        // referenced assembly carries a SyntaxTree that belongs to
        // the declaring compilation, not ours. Calling
        // GetSemanticModel on that tree used to throw ArgumentException
        // or fall back to the Erasable member identifier. Source
        // ProjectReferences should now inline through the referenced
        // compilation.
        var result = TranspileHelper.TranspileWithLibrary(
            """
            using Metano.Annotations;

            [Erasable]
            public static class Constants
            {
                [Inline]
                public static int Answer => 42;
            }
            """,
            """
            [assembly: TranspileAssembly]

            public class Asker
            {
                public int Ask() => Constants.Answer;
            }
            """
        );

        var output = result["asker.ts"];
        await Assert.That(output).Contains("return 42;");
        await Assert.That(output).DoesNotContain("return answer;");
    }

    [Test]
    public async Task Inline_CrossAssembly_DomBindingCatalogEntry_StaysStringLiteral()
    {
        var result = TranspileHelper.TranspileWithLibrary(
            """
            using Metano.Annotations;
            using Metano.Annotations.TypeScript;

            [External, NoEmit]
            public static class Js
            {
                [Name("document")]
                public static Document Document => throw null!;
            }

            [NoEmit, Name("HTMLElement")]
            public abstract class HtmlElement
            {
                public string Id { get; set; } = "";
            }

            [NoEmit, Name("HTMLDivElement")]
            public abstract class HtmlDivElement : HtmlElement;

            [External]
            public abstract class Document
            {
                public HtmlElement CreateElement(string elementName) => throw null!;
            }

            [Transpile, Erasable]
            public static class DocumentExtensions
            {
                extension(Document document)
                {
                    [Inline]
                    public TElement CreateElement<TElement>(HtmlElementType.Of<TElement> type)
                        where TElement : HtmlElement
                    {
                        return (TElement)document.CreateElement(type.TagName);
                    }
                }
            }

            [Transpile, Erasable]
            public static class HtmlElementType
            {
                [PlainObject]
                public sealed record Of<T>(string TagName)
                    where T : HtmlElement;

                [Inline]
                public static Of<HtmlDivElement> Div => new("div");
            }
            """,
            """
            [assembly: TranspileAssembly]

            public class Renderer
            {
                public HtmlElement Create() => Js.Document.CreateElement(HtmlElementType.Div);
            }
            """
        );

        var output = result["renderer.ts"];
        await Assert.That(output).Contains("return document.createElement(\"div\");");
        await Assert.That(output).DoesNotContain("document.createElement(div)");
        await Assert.That(output).DoesNotContain("new HtmlElementType.Of");
    }

    // ─── Diagnostics (MS0016) ─────────────────────────────────

    [Test]
    public async Task Inline_OnInstanceField_EmitsMs0016()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public class Catalog
            {
                [Inline]
                public readonly int X = 42;
            }
            """
        );

        var ms0016 = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.InvalidInline);
        await Assert.That(ms0016).IsNotNull();
        await Assert.That(ms0016!.Message).Contains("static");
    }

    [Test]
    public async Task Inline_OnMutableField_EmitsMs0016()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public static class Catalog
            {
                [Inline]
                public static int X = 42;
            }
            """
        );

        var ms0016 = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.InvalidInline);
        await Assert.That(ms0016).IsNotNull();
        await Assert.That(ms0016!.Message).Contains("readonly");
    }

    [Test]
    public async Task Inline_OnFieldWithoutInitializer_EmitsMs0016()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public static class Catalog
            {
                [Inline]
                public static readonly int X;

                static Catalog() { X = 42; }
            }
            """
        );

        var ms0016 = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.InvalidInline);
        await Assert.That(ms0016).IsNotNull();
        await Assert.That(ms0016!.Message).Contains("initializer");
    }

    [Test]
    public async Task Inline_OnBlockBodiedProperty_EmitsMs0016()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public static class Catalog
            {
                [Inline]
                public static int X
                {
                    get { return 42; }
                }
            }
            """
        );

        var ms0016 = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.InvalidInline);
        await Assert.That(ms0016).IsNotNull();
        await Assert.That(ms0016!.Message).Contains("expression-bodied");
    }

    [Test]
    public async Task Inline_SelfReferentialProperty_EmitsMs0016()
    {
        // A property whose initializer refers to itself would cause
        // unbounded substitution. The validator detects the cycle
        // via DFS and surfaces MS0016 before extraction runs.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public static class Loop
            {
                [Inline]
                public static string X => X;
            }
            """
        );

        var ms0016 = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.InvalidInline);
        await Assert.That(ms0016).IsNotNull();
        await Assert.That(ms0016!.Message).Contains("cycle");
    }

    [Test]
    public async Task Inline_MutualRecursion_EmitsMs0016()
    {
        // Two [Inline] members pointing at each other also form a
        // cycle. The DFS flags at least one of the two.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            public static class Loop
            {
                [Inline]
                public static string A => B;

                [Inline]
                public static string B => A;
            }
            """
        );

        var ms0016 = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.InvalidInline);
        await Assert.That(ms0016).IsNotNull();
        await Assert.That(ms0016!.Message).Contains("cycle");
    }

    [Test]
    public async Task Inline_Property_CascadesThroughAnotherInline()
    {
        var result = TranspileHelper.Transpile(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [Erasable]
            public static class Colors
            {
                [Inline]
                public static string Primary => "#112233";

                [Inline]
                public static string Highlight => Primary;
            }

            public class Theme
            {
                public string Accent() => Colors.Highlight;
            }
            """
        );

        var output = result["theme.ts"];
        await Assert.That(output).Contains("return \"#112233\";");
        await Assert.That(output).DoesNotContain("Colors.");
    }
}
