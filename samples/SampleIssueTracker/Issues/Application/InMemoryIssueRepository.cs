using SampleIssueTracker.Issues.Domain;
using SampleIssueTracker.SharedKernel;

namespace SampleIssueTracker.Issues.Application;

public class InMemoryIssueRepository : IIssueRepository
{
    private readonly List<Issue> _issues = [];

    public Task<Issue?> GetByIdAsync(IssueId id) =>
        Task.FromResult(_issues.FirstOrDefault(issue => issue.Id == id));

    public Task<IReadOnlyList<Issue>> ListAsync() =>
        Task.FromResult<IReadOnlyList<Issue>>(_issues.OrderBy(issue => issue.CreatedAt).ToList());

    public Task SaveAsync(Issue entity)
    {
        var existingIndex = _issues.FindIndex(issue => issue.Id == entity.Id);

        if (existingIndex >= 0)
        {
            _issues[existingIndex] = entity;
        }
        else
        {
            _issues.Add(entity);
        }

        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(IssueId id) => await GetByIdAsync(id) is not null;

    public Task<IReadOnlyList<Issue>> ListBySprintAsync(string sprintKey) =>
        Task.FromResult<IReadOnlyList<Issue>>(
            _issues
                .Where(issue => issue.SprintKey == sprintKey)
                .OrderByDescending(issue => issue.Priority)
                .ThenBy(issue => issue.Title)
                .ToList()
        );

    public Task<PageResult<Issue>> SearchAsync(
        IssueStatus? status,
        IssuePriority? priority,
        UserId? assigneeId,
        string? sprintKey,
        PageRequest page
    )
    {
        var filtered = _issues
            .Where(issue => status is null || issue.Status == status)
            .Where(issue => priority is null || issue.Priority == priority)
            .Where(issue => assigneeId is null || issue.AssigneeId == assigneeId)
            .Where(issue => sprintKey is null || issue.SprintKey == sprintKey)
            .OrderByDescending(issue => issue.Priority)
            .ThenBy(issue => issue.Title)
            .ToList();

        var items = filtered.Skip(page.Skip).Take(page.SafeSize).ToList();

        return Task.FromResult(new PageResult<Issue>(items, filtered.Count, page));
    }
}
