using Metano.Annotations;
using SampleCounterV4.Mvu.Widgets;
using ColumnWidget = SampleCounterV4.Mvu.Widgets.Column;

namespace SampleCounterV4.Mvu;

// Simulated source-generator output: thin factory facade so consumers can
// `using static SampleCounterV4.Mvu.UI;` and write `Column(...)`, `Row(...)`,
// etc. without `new`. A real generator would scan widget classes and emit
// one factory per public ctor.
[Erasable]
public static class Ui
{
    [ObjectArgs, Name(TargetLanguage.TypeScript, nameof(Column))]
    public static ColumnWidget Column(int gap, Widget[] children) => new(gap, children);

    [ObjectArgs]
    public static Row Row(int gap, Widget[] children) => new(gap, children);

    [ObjectArgs]
    public static Text Text(string content) => new(content);

    [ObjectArgs]
    public static Heading Heading(string content, int level = 1) => new(content, level);

    [ObjectArgs]
    public static Button Button(string label, Action onPressed) => new(label, onPressed);
}
