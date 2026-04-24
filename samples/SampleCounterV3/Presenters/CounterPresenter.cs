using SampleCounterV2.Models;
using SampleCounterV2.Views;

namespace SampleCounterV2.Presenters;

public sealed class CounterPresenter(ICounterView view, Counter initialState)
{
    private Counter _state = initialState;

    public void StartApplication(string containerName)
    {
        var renderer = new Renderer(containerName);
        renderer.Render(view);

        view.OnButtonClick = OnButtonClicked;
        view.ShowCounter(_state.Count);
    }

    private void OnButtonClicked()
    {
        _state = _state.Increment();
        view.ShowCounter(_state.Count);
    }
}
