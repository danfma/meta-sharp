using Metano.TypeScript.DOM;

namespace SampleCounterV4.Mvu.Widgets;

public sealed class Heading(string content, int level = 1) : Widget
{
    public override HtmlElement Render()
    {
        var headingType = level switch
        {
            1 => HtmlElementType.H1,
            2 => HtmlElementType.H2,
            3 => HtmlElementType.H3,
            4 => HtmlElementType.H4,
            5 => HtmlElementType.H5,
            _ => HtmlElementType.H6,
        };

        var sizeEm = level switch
        {
            1 => 2.0,
            2 => 1.5,
            3 => 1.25,
            4 => 1.0,
            5 => 0.875,
            _ => 0.75,
        };

        var element = Js.Document.CreateElement(headingType);
        element.TextContent = content;
        element.ApplyStyle($"font-size:{sizeEm}em");

        return element;
    }
}
