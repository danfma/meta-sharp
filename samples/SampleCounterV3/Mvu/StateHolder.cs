namespace SampleCounterV3.Mvu;

public sealed class StateHolder<TState>(TState initial)
{
    private TState _state = initial;

    public TState State => _state;

    public Action? OnChange { get; set; }

    public void Update(Func<TState, TState> reducer)
    {
        _state = reducer(_state);
        OnChange?.Invoke();
    }
}
