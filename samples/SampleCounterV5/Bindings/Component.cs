using Metano.Annotations;

namespace SampleCounterV5.Bindings;

/// <summary>
/// Strongly-typed wrapper over Inferno's stateful <c>Component</c> class.
/// Subclasses override <see cref="Render"/> to return the virtual-DOM
/// tree for the current props + state. The <c>[Import]</c> + stub-throw
/// pattern lets the C# compiler accept references while the TS emit
/// resolves to <c>import { Component } from "inferno"</c> in every
/// consumer file that subclasses it.
/// </summary>
[Import(name: "Component", from: "inferno", Version = "^9.0.0")]
public abstract class Component<TProps, TState>
{
    public TProps Props { get; } = default!;
    public TState State { get; } = default!;

    [Name("setState")]
    public void SetState(TState newState) => throw new NotSupportedException();

    [Name("render")]
    public virtual InfernoElement Render() => throw new NotSupportedException();
}

/// <summary>
/// Empty-props marker for components that take no props.
/// </summary>
[PlainObject]
public sealed record EmptyProps;
