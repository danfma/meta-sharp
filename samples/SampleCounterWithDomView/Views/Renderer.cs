using Metano.TypeScript.DOM;

namespace SampleCounterWithDomView.Views;

public sealed class Renderer
{
    private readonly HtmlElement _container;

    public Renderer(string containerId)
    {
        _container = Js.Document.GetElementById(containerId) ?? CreateElement(containerId);
    }

    private HtmlElement CreateElement(string containerId)
    {
        var element = Js.Document.CreateElement<HtmlDivElement>();
        element.Id = containerId;
        Js.Document.Body.Append(element);

        return element;
    }

    public void Render(IView view) { }
}
