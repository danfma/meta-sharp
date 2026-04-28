using SampleCounterV4.Mvu.Widgets;

namespace SampleCounterV4.Mvu;

// Simulated source-generator output: thin factory facade so consumers can
// `using static SampleCounterV4.Mvu.UI;` and write `Column(...)`, `Row(...)`,
// etc. without `new`. A real generator would scan widget classes and emit
// one factory per public ctor.
public static class UI
{
    public static Column Column(int gap, Widget[] children) => new(gap, children);

    public static Row Row(int gap, Widget[] children) => new(gap, children);

    public static Text Text(string content) => new(content);

    public static Heading Heading(string content, int level = 1) => new(content, level);

    public static Button Button(string label, Action onPressed) => new(label, onPressed);
}
