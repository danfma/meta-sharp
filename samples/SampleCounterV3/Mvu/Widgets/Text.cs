using Metano.TypeScript.DOM;

namespace SampleCounterV3.Mvu.Widgets;

public sealed class Text(string content) : IWidget
{
    public HtmlElement Build()
    {
        var span = Js.Document.CreateElement(HtmlElementType.Span);
        span.TextContent = content;
        return span;
    }
}
