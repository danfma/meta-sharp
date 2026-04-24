using Metano.Annotations;
using Metano.Annotations.TypeScript;

namespace Metano.TypeScript.DOM;

[External, NoEmit]
public static class Js
{
    [Name("window")]
    public static Window Window => throw new NotSupportedException();

    [Name("document")]
    public static Document Document => throw new NotSupportedException();
}
