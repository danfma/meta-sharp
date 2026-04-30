using Metano.Annotations;
using ButtonWidget = SampleCounterV3.Mvu.Widgets.Button;
using ColumnWidget = SampleCounterV3.Mvu.Widgets.Column;
using HeadingWidget = SampleCounterV3.Mvu.Widgets.Heading;
using RowWidget = SampleCounterV3.Mvu.Widgets.Row;
using TextWidget = SampleCounterV3.Mvu.Widgets.Text;

namespace SampleCounterV3.Mvu;

// Simulated source-generator output: thin factory facade so consumers can
// `using static SampleCounterV3.Mvu.UI;` and write `Column(...)`, `Row(...)`,
// etc. without `new`. A real generator would scan widget classes and emit
// one factory per public ctor.
[Erasable]
public static class Ui
{
    [ObjectArgs, Name(TargetLanguage.TypeScript, nameof(Column))]
    public static ColumnWidget Column(int gap, params Widget[] children) => new(gap, children);

    [ObjectArgs, Name(TargetLanguage.TypeScript, nameof(Row))]
    public static RowWidget Row(int gap, params Widget[] children) => new(gap, children);

    [ObjectArgs, Name(TargetLanguage.TypeScript, nameof(Text))]
    public static TextWidget Text(string content) => new(content);

    [ObjectArgs, Name(TargetLanguage.TypeScript, nameof(Heading))]
    public static HeadingWidget Heading(string content, int level = 1) => new(content, level);

    [ObjectArgs, Name(TargetLanguage.TypeScript, nameof(Button))]
    public static ButtonWidget Button(string label, Action onPressed) => new(label, onPressed);
}
