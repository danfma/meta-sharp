using Metano.Annotations;

namespace Metano.TypeScript.DOM;

[ExportedAsModule, NoEmit]
public static class Js
{
    [Name("window")]
    public static Window Window => throw new NotSupportedException();

    [Name("document")]
    public static Document Document => throw new NotSupportedException();
}

[NoEmit]
public abstract class EventTarget;

[NoEmit]
public abstract class Window : EventTarget
{
    public Document Document => throw new NotSupportedException();
}

[NoEmit]
public abstract class Node : EventTarget;

[NoEmit]
public abstract class Element : Node
{
    public string Id { get; set; } = "";

    public void Append(params Node[] nodes) => throw new NotSupportedException();
}

[NoEmit, Name("HTMLElement")]
public abstract class HtmlElement : Element;

[NoEmit]
public abstract class Document : Node
{
    public HtmlBodyElement Body => throw new NotSupportedException();

    public HtmlElement? GetElementById(string id) => throw new NotSupportedException();

    public HtmlElement CreateElement(string elementName) => throw new NotSupportedException();
}

[NoEmit]
public abstract class HtmlBodyElement : HtmlElement { }

[NoEmit, Name("HTMLDivElement")]
public abstract class HtmlDivElement : HtmlElement, IHtmlElement<HtmlDivElement>
{
    public static string ElementName => "div";
}

[NoEmit]
public interface IHtmlElement<TSelf>
    where TSelf : IHtmlElement<TSelf>
{
    public static abstract string ElementName { get; }
}

public static class ElementFactoryExtension
{
    extension(Document document)
    {
        public TElement CreateElement<TElement>()
            where TElement : HtmlElement, IHtmlElement<TElement>
        {
            return (TElement)document.CreateElement(TElement.ElementName);
        }
    }
}
