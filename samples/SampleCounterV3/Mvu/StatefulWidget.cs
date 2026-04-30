using Metano.TypeScript.DOM;

namespace SampleCounterV3.Mvu;

public abstract class StatefulWidget<TState> : Widget
{
    private BuildContext<TState>? _context;

    public abstract TState Initial();

    protected abstract Widget Build(BuildContext<TState> context);

    public void Bind(BuildContext<TState> context) => _context = context;

    public override HtmlElement Render()
    {
        if (_context == null)
            throw new InvalidOperationException("Widget not bound to a context.");

        return Build(_context).Render();
    }
}
