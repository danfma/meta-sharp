using Metano.Annotations;
using Metano.Annotations.TypeScript;

namespace Metano.TypeScript.DOM;

[External]
public abstract class Element : Node
{
    public string Id { get; set; } = "";

    [Name("innerHTML")]
    public string? InnerHtml { get; set; }

    [Name("onclick")]
    public MouseEventListener? OnClick { get; set; }

    public void Append(params Node[] nodes) => throw new NotSupportedException();
}

// [Transpile, External]
public delegate void MouseEventListener(MouseEvent @event);

[Transpile, External]
public sealed class MouseEvent;
