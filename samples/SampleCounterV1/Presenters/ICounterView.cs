using SampleCounterV1.Views;

namespace SampleCounterV1.Presenters;

public interface ICounterView : IView
{
    Action? OnButtonClick { get; set; }

    void ShowCounter(int counter);
}
