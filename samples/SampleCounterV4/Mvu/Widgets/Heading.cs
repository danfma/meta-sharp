using Metano.TypeScript.DOM;

namespace SampleCounterV4.Mvu.Widgets;

public sealed class Heading(string content, int level = 1) : Widget
{
    public override HtmlElement Render()
    {
        var sizeEm = level switch
        {
            1 => 2.0,
            2 => 1.5,
            3 => 1.25,
            4 => 1.0,
            5 => 0.875,
            _ => 0.75,
        };

        var span = Js.Document.CreateElement(HtmlElementType.Span);
        span.TextContent = content;
        span.ApplyStyle($"display:block;font-weight:bold;font-size:{sizeEm}em");

        return span;
    }
}
