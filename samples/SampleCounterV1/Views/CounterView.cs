using Metano.TypeScript.DOM;
using SampleCounterV1.Presenters;

namespace SampleCounterV1.Views;

public sealed class CounterView : ICounterView
{
    private HtmlSpanElement? _text;

    public Action? OnButtonClick { get; set; }

    public void Render(HtmlElement container)
    {
        var root = Js.Document.CreateElement(HtmlElementType.Div);
        container.Append(root);

        var text = Js.Document.CreateElement(HtmlElementType.Span);
        text.InnerHtml = "0";
        root.Append(text);
        _text = text;

        var button = Js.Document.CreateElement(HtmlElementType.Button);
        button.InnerHtml = "Click me";
        button.OnClick = _ => OnButtonClick?.Invoke();
        root.Append(button);
    }

    public void ShowCounter(int counter)
    {
        _text?.InnerHtml = counter.ToString();
    }
}
