namespace SampleIssueTracker.SharedKernel;

public record PageRequest(int Number = 1, int Size = 20)
{
    public int SafeNumber => Math.Max(1, Number);

    public int SafeSize => Math.Max(1, Size);

    public int Skip => (SafeNumber - 1) * SafeSize;
}
