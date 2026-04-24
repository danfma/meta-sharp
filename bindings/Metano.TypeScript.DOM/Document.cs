using Metano.Annotations.TypeScript;

namespace Metano.TypeScript.DOM;

[External]
public abstract class Document : Node
{
    public HtmlBodyElement Body => throw new NotSupportedException();

    public HtmlElement? GetElementById(string id) => throw new NotSupportedException();

    public HtmlElement CreateElement(string elementName) => throw new NotSupportedException();
}
