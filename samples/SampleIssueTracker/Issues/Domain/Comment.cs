using SampleIssueTracker.SharedKernel;

namespace SampleIssueTracker.Issues.Domain;

public record Comment(UserId AuthorId, string Message, DateTimeOffset CreatedAt, bool IsSystem = false)
{
    public static Comment System(string message, DateTimeOffset createdAt) =>
        new(UserId.System(), message, createdAt, true);
}
