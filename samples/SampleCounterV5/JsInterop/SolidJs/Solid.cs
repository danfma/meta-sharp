using Metano.Annotations;
using Metano.Annotations.TypeScript;

namespace SampleCounterV5.JsInterop.SolidJs;

[NoContainer]
public static class Solid
{
    [Import("createSignal", from: "solid-js")]
    private static IRawSignal<T> CreateRawSignal<T>(T value)
    {
        throw new NotSupportedException("Only for TypeScript");
    }

    public static ISignal<T> CreateSignal<T>(T value)
    {
        var signal = CreateRawSignal(value);

        return new SignalWrapper<T>(signal);
    }

    [Import("createEffect", from: "solid-js")]
    public static void CreateEffect(Action action)
    {
        throw new NotSupportedException("Only for TypeScript");
    }
}

[External, Import("Signal", from: "solid-js")]
public interface IRawSignal<T>;
