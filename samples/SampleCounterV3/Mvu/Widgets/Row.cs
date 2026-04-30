using Metano.TypeScript.DOM;

namespace SampleCounterV3.Mvu.Widgets;

public sealed class Row(int gap, Widget[] children) : Widget
{
    public override HtmlElement Render()
    {
        var div = Js.Document.CreateElement(HtmlElementType.Div);
        div.ApplyStyle($"display:flex;flex-direction:row;gap:{gap}px");

        foreach (var child in children)
        {
            div.Append(child.Render());
        }

        return div;
    }
}
