using Metano.TypeScript.DOM;

namespace SampleCounterV3.Mvu.Widgets;

public sealed class Column(IWidget[] children) : IWidget
{
    public HtmlElement Build()
    {
        var div = Js.Document.CreateElement(HtmlElementType.Div);
        for (var i = 0; i < children.Length; i++)
            div.Append(children[i].Build());
        return div;
    }
}
