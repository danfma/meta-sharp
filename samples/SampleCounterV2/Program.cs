using SampleCounterV2.Models;
using SampleCounterV2.Presenters;
using SampleCounterV2.Views;

var view = new CounterView();
var presenter = new CounterPresenter(view, Counter.Zero);

presenter.StartApplication("root");
