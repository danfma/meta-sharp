namespace SampleCounterV5.JsInterop.SolidJs;

public interface ISignal<T>
{
    public T Value { get; }

    public void Set(T value);
    public void Set(Func<T, T> updater);
}
