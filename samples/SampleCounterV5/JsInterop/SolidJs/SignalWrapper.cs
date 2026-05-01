using Metano.Annotations;

namespace SampleCounterV5.JsInterop.SolidJs;

public sealed class SignalWrapper<T>(IRawSignal<T> signal) : ISignal<T>
{
    public T Value
    {
        get => ReadValue(signal);
        set => Set(value);
    }

    [Emit("$0[0]()")]
    private static T ReadValue(IRawSignal<T> signal)
    {
        throw new NotSupportedException("Only for TypeScript");
    }

    [Emit("$0[1]($1)")]
    private static void WriteValue(IRawSignal<T> signal, Func<T, T> updater)
    {
        throw new NotSupportedException("Only for TypeScript");
    }

    public void Set(T value)
    {
        WriteValue(signal, _ => value);
    }

    public void Set(Func<T, T> updater)
    {
        WriteValue(signal, updater);
    }
}
