using Metano.TypeScript.DOM;

namespace SampleCounterV4.Mvu.Widgets;

// Explicit ctor (instead of primary ctor) is a workaround for #158: a
// primary-ctor parameter used as the operand of a `switch` expression is
// emitted as a bare identifier in TS instead of `this._field`.
public sealed class Heading : Widget
{
    private readonly string _content;
    private readonly int _level;

    public Heading(string content, int level = 1)
    {
        _content = content;
        _level = level;
    }

    public override HtmlElement Render()
    {
        var sizeEm = _level switch
        {
            1 => 2.0,
            2 => 1.5,
            3 => 1.25,
            4 => 1.0,
            5 => 0.875,
            _ => 0.75,
        };

        var span = Js.Document.CreateElement(HtmlElementType.Span);
        span.TextContent = _content;
        span.ApplyStyle($"display:block;font-weight:bold;font-size:{sizeEm}em");

        return span;
    }
}
