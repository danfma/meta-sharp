using SampleCounterV1.Models;
using SampleCounterV1.Presenters;
using SampleCounterV1.Views;

var view = new CounterView();
var presenter = new CounterPresenter(view, Counter.Zero);

presenter.StartApplication("root");
