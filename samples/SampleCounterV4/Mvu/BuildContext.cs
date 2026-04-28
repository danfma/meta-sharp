namespace SampleCounterV4.Mvu;

public sealed class BuildContext<TState>(TState state, Action<Func<TState, TState>> updater)
{
    public TState State => state;

    public void SetState(Func<TState, TState> reducer) => updater(reducer);
}
