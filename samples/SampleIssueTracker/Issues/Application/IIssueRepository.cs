using SampleIssueTracker.Issues.Domain;
using SampleIssueTracker.SharedKernel;

namespace SampleIssueTracker.Issues.Application;

public interface IIssueRepository
{
    Task<Issue?> GetByIdAsync(IssueId id);

    Task<IReadOnlyList<Issue>> ListAsync();

    Task SaveAsync(Issue issue);

    Task<bool> ExistsAsync(IssueId id);

    Task<IReadOnlyList<Issue>> ListBySprintAsync(string sprintKey);

    Task<PageResult<Issue>> SearchAsync(
        IssueStatus? status,
        IssuePriority? priority,
        UserId? assigneeId,
        string? sprintKey,
        PageRequest page
    );
}
