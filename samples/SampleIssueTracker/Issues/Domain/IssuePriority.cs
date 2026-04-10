using Metano.Annotations;

namespace SampleIssueTracker.Issues.Domain;

[StringEnum]
public enum IssuePriority
{
    [Name("low")] Low,
    [Name("medium")] Medium,
    [Name("high")] High,
    [Name("urgent")] Urgent,
}
