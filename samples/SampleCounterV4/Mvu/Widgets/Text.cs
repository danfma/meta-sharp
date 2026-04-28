using Metano.TypeScript.DOM;

namespace SampleCounterV4.Mvu.Widgets;

public sealed class Text(string content) : Widget
{
    public override HtmlElement Render()
    {
        var span = Js.Document.CreateElement(HtmlElementType.Span);
        span.TextContent = content;

        return span;
    }
}
