using MetaSharp;

namespace SampleIssueTracker.Issues.Domain;

[StringEnum]
public enum IssueType
{
    [Name("story")] Story,
    [Name("bug")] Bug,
    [Name("chore")] Chore,
    [Name("spike")] Spike,
}
