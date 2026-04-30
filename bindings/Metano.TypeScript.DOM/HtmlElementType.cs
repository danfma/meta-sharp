using Metano.Annotations;

namespace Metano.TypeScript.DOM;

[Transpile, NoContainer]
public static class HtmlElementType
{
    [PlainObject]
    public sealed record Of<T>(string TagName)
        where T : HtmlElement;

    [Inline]
    public static Of<HtmlDivElement> Div => new("div");

    [Inline]
    public static readonly Of<HtmlSpanElement> Span = new("span");

    [Inline]
    public static readonly Of<HtmlButtonElement> Button = new("button");

    [Inline]
    public static readonly Of<HtmlHeadingElement> H1 = new("h1");

    [Inline]
    public static readonly Of<HtmlHeadingElement> H2 = new("h2");

    [Inline]
    public static readonly Of<HtmlHeadingElement> H3 = new("h3");

    [Inline]
    public static readonly Of<HtmlHeadingElement> H4 = new("h4");

    [Inline]
    public static readonly Of<HtmlHeadingElement> H5 = new("h5");

    [Inline]
    public static readonly Of<HtmlHeadingElement> H6 = new("h6");
}
