namespace SampleCounter;

public sealed class CounterPresenter
{
    private readonly ICounterView _view;
    private Counter _counter = Counter.Zero;

    public CounterPresenter(ICounterView view)
    {
        _view = view;

        Initialize();
    }

    private void Initialize()
    {
        _view.DisplayCounter(_counter);
    }

    public void Increment()
    {
        _counter = _counter.Increment();
        DisplayCounter();
    }

    public void Decrement()
    {
        _counter = _counter.Decrement();
        DisplayCounter();
    }

    private void DisplayCounter()
    {
        _view.DisplayCounter(_counter);
    }
}
