using Metano.TypeScript.DOM;

namespace SampleCounterV3.Mvu.Widgets;

public sealed class Column(int gap, Widget[] children) : Widget
{
    public override HtmlElement Render()
    {
        var div = Js.Document.CreateElement(HtmlElementType.Div);
        div.ApplyStyle($"display:flex;flex-direction:column;gap:{gap}px");

        foreach (var child in children)
        {
            div.Append(child.Render());
        }

        return div;
    }
}
