using Metano.TypeScript.DOM;

namespace SampleCounterV3.Mvu;

public delegate IWidget ViewFn<TState>(TState state, Action<TState> setState);

public static class App
{
    public static void Mount<TState>(string containerId, TState initialState, ViewFn<TState> view)
    {
        var container = ResolveContainer(containerId);
        var holder = new StateHolder<TState>(initialState);

        // Bind the setState delegate once. Each `holder.Set` method-group
        // conversion would otherwise allocate a fresh closure on every
        // render — captured into a local, both the initial and every
        // subsequent render reuse the same instance and keep a stable
        // identity for callers that compare delegates.
        var setState = holder.Set;
        var render = () => Apply(view(holder.State, setState), container);

        holder.OnChange = render;
        render();
    }

    private static void Apply(IWidget root, HtmlElement container)
    {
        container.InnerHtml = "";
        container.Append(root.Build());
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
