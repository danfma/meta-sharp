namespace SampleIssueTracker.Issues.Domain;

public record IssueId(string Value)
{
    public static IssueId New() => new(Guid.NewGuid().ToString("N"));

    public override string ToString() => Value;
}
