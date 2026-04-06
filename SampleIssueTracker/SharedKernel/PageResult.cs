namespace SampleIssueTracker.SharedKernel;

public record PageResult<T>(IReadOnlyList<T> Items, int TotalCount, PageRequest Page)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)Page.SafeSize);

    public bool HasNextPage => Page.SafeNumber < TotalPages;
}
