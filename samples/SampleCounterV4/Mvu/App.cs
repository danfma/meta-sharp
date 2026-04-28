using Metano.TypeScript.DOM;

namespace SampleCounterV4.Mvu;

public static class App
{
    public static void Run<TState>(string containerId, StatefulWidget<TState> widget)
    {
        var holder = new StateHolder<TState>(widget.Initial());
        var container = ResolveContainer(containerId);
        var render = () =>
        {
            widget.Bind(new BuildContext<TState>(holder.State, holder.Update));

            container.InnerHtml = "";
            container.Append(widget.Render());
        };

        holder.OnChange = render;
        render();
    }

    private static HtmlElement ResolveContainer(string containerId)
    {
        var existing = Js.Document.GetElementById(containerId);

        if (existing != null)
            return existing;

        var element = Js.Document.CreateElement(HtmlElementType.Div);
        element.Id = containerId;
        Js.Document.Body.Append(element);

        return element;
    }
}
