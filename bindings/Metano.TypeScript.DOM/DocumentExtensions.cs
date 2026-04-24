using Metano.Annotations;

namespace Metano.TypeScript.DOM;

[Transpile, Erasable]
public static class DocumentExtensions
{
    extension(Document document)
    {
        [Inline]
        public TElement CreateElement<TElement>(HtmlElementType.Of<TElement> type)
            where TElement : HtmlElement
        {
            return (TElement)document.CreateElement(type.TagName);
        }
    }
}
