using Metano.Annotations;

namespace Metano.TypeScript.DOM;

[NoEmit]
public abstract class Window : EventTarget
{
    public Document Document => throw new NotSupportedException();
}
