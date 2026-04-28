using SampleCounterV4.Models;
using SampleCounterV4.Mvu;

namespace SampleCounterV4.Components;

// Simulated source-generator output: a real generator inspecting
// `[Component]`-annotated StatefulWidgets would emit this Mount entry point
// per component, threading the state type into App.Run.
public sealed partial class CounterApp
{
    public static void Mount(string containerId) =>
        App.Run<Counter>(containerId, new CounterApp());
}
