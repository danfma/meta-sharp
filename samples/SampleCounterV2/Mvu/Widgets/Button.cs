using Metano.TypeScript.DOM;

namespace SampleCounterV2.Mvu.Widgets;

public sealed class Button(string label, Action onPressed) : IWidget
{
    public HtmlElement Build()
    {
        var btn = Js.Document.CreateElement(HtmlElementType.Button);
        btn.TextContent = label;
        btn.OnClick = _ => onPressed();

        return btn;
    }
}
