namespace SampleCounterV2.Mvu;

public sealed class StateHolder<TState>(TState initial)
{
    private TState _state = initial;

    public TState State => _state;

    public Action? OnChange { get; set; }

    public void Set(TState next)
    {
        _state = next;
        OnChange?.Invoke();
    }
}
