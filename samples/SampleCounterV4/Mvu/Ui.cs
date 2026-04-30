using Metano.Annotations;
using SampleCounterV4.Inferno;
using static SampleCounterV4.Inferno.Inferno;

namespace SampleCounterV4.Mvu;

/// <summary>
/// JSX-flavored widget facade. <c>[NoContainer]</c> flattens the qualifier
/// at the call site (<c>Ui.Column(...)</c> → <c>Column(...)</c>) while
/// <c>[ObjectArgs]</c> on each factory turns positional + named C#
/// arguments into a single TS object-literal — the call surface looks
/// just like JSX (<c>Column({ gap: 12, children: [...] })</c>).
/// Internally each factory routes to Inferno's <c>createElement</c>.
/// </summary>
[NoContainer]
public static class Ui
{
    [ObjectArgs, Name(TargetLanguage.TypeScript, nameof(Column))]
    public static InfernoElement Column(int gap = 0, params InfernoElement[] children) =>
        H("div", new DomProps(ClassName: $"column gap-{gap}"), children);

    [ObjectArgs, Name(TargetLanguage.TypeScript, nameof(Row))]
    public static InfernoElement Row(int gap = 0, params InfernoElement[] children) =>
        H("div", new DomProps(ClassName: $"row gap-{gap}"), children);

    [ObjectArgs, Name(TargetLanguage.TypeScript, nameof(Text))]
    public static InfernoElement Text(string content) =>
        H("span", new DomProps(ClassName: "text"), TextNode(content));

    [ObjectArgs, Name(TargetLanguage.TypeScript, nameof(Heading))]
    public static InfernoElement Heading(string content, int level = 1) =>
        H($"h{level}", new DomProps(ClassName: "heading"), TextNode(content));

    [ObjectArgs, Name(TargetLanguage.TypeScript, nameof(Button))]
    public static InfernoElement Button(string label, Action onClick) =>
        H("button", new ButtonProps(ClassName: "btn", OnClick: onClick), TextNode(label));

    /// <summary>
    /// Inferno accepts a string child directly — no wrapping needed —
    /// but the Metano IR requires a uniform <see cref="InfernoElement"/>
    /// child type, so we lower string content to a literal element via
    /// the runtime cast helper.
    /// </summary>
    [Emit("$0 as any")]
    private static extern InfernoElement TextNode(string content);
}
