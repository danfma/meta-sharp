using SampleCounterV3.Models;
using SampleCounterV3.Mvu;

namespace SampleCounterV3.Components;

// Simulated source-generator output: a real generator inspecting
// `[Component]`-annotated StatefulWidgets would emit this Mount entry point
// per component, threading the state type into App.Run.
public sealed partial class CounterApp
{
    public static void Mount(string containerId) =>
        App.Run<Counter>(containerId, new CounterApp());
}
