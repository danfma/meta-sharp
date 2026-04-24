using SampleCounterV2.Views;

namespace SampleCounterV2.Presenters;

public interface ICounterView : IView
{
    Action? OnButtonClick { get; set; }

    void ShowCounter(int counter);
}
