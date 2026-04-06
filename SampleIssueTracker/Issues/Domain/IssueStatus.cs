using MetaSharp;

namespace SampleIssueTracker.Issues.Domain;

[StringEnum]
public enum IssueStatus
{
    [Name("backlog")] Backlog,
    [Name("ready")] Ready,
    [Name("in-progress")] InProgress,
    [Name("in-review")] InReview,
    [Name("done")] Done,
    [Name("cancelled")] Cancelled,
}
