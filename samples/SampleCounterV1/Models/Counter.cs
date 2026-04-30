namespace SampleCounterV1.Models;

public sealed record Counter(int Count)
{
    public static readonly Counter Zero = new(0);

    public Counter Increment() => this with { Count = Count + 1 };
}
