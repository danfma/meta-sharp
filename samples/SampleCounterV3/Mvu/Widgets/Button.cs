using Metano.TypeScript.DOM;

namespace SampleCounterV3.Mvu.Widgets;

public sealed class Button(string label, Action onPressed) : Widget
{
    public override HtmlElement Render()
    {
        var btn = Js.Document.CreateElement(HtmlElementType.Button);
        btn.TextContent = label;
        btn.OnClick = _ => onPressed();

        return btn;
    }
}
