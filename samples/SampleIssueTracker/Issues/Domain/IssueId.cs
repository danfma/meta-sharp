using Metano.Annotations;

namespace SampleIssueTracker.Issues.Domain;

[Branded]
public readonly record struct IssueId(string Value)
{
    public static IssueId New() => new(Guid.NewGuid().ToString("N"));

    public override string ToString() => Value;
}
