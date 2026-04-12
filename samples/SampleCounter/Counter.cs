namespace SampleCounter;

public readonly record struct Counter(int Count)
{
    public static Counter Zero => new(0);

    public Counter Increment() => new(Count + 1);

    public Counter Decrement() => new(Count - 1);
}
