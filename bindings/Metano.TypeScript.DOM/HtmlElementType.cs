using Metano.Annotations;

namespace Metano.TypeScript.DOM;

[Transpile, Erasable]
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
}
