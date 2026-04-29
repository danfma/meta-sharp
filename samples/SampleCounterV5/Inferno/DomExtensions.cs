using Metano.TypeScript.DOM;

namespace SampleCounterV5.Inferno;

public static class DomExtensions
{
    extension(Document document)
    {
        public HtmlElement GetOrCreateElementById(string id)
        {
            return document.GetElementById(id) ?? document.CreateElement(HtmlElementType.Div);
        }
    }
}
