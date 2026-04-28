using SampleCounterV3.Models;
using SampleCounterV3.Mvu;
using SampleCounterV3.Mvu.Widgets;

App.Mount(
    containerId: "root",
    initialState: Counter.Zero,
    view: (state, setState) =>
        new Column([
            new Text(state.Count.ToString()),
            new Button("Click me", onPressed: () => setState(state.Increment())),
        ])
);
