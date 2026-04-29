using Metano.Annotations;
using Metano.Annotations.TypeScript;

namespace Metano.TypeScript.DOM;

[External]
public abstract class Window : EventTarget
{
    public Document Document => throw new NotSupportedException();
}
