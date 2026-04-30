using Metano.Annotations;
using Metano.TypeScript.DOM;

namespace SampleCounterV3.Mvu;

[Transpile, Erasable]
public static class StyleSupport
{
    [Emit("$0.setAttribute('style', $1)")]
    public static extern void ApplyStyle(this HtmlElement element, string style);
}
