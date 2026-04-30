using Metano.Annotations;

namespace Metano.TypeScript.DOM;

[Transpile, NoContainer]
public static class DocumentExtensions
{
    extension(Document document)
    {
        [Inline(InlineMode.Substitute)]
        public TElement CreateElement<TElement>(HtmlElementType.Of<TElement> type)
            where TElement : HtmlElement
        {
            return (TElement)document.CreateElement(type.TagName);
        }
    }
}
