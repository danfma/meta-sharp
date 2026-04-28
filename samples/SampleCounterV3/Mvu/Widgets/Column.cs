using Metano.TypeScript.DOM;

namespace SampleCounterV3.Mvu.Widgets;

public sealed class Column(IWidget[] children) : IWidget
{
    public HtmlElement Build()
    {
        var div = Js.Document.CreateElement(HtmlElementType.Div);

        foreach (var child in children)
        {
            div.Append(child.Build());
        }

        return div;
    }
}
