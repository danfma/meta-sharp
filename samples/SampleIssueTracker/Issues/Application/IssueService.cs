using SampleIssueTracker.Issues.Domain;
using SampleIssueTracker.SharedKernel;

namespace SampleIssueTracker.Issues.Application;

public class IssueService(IIssueRepository repository)
{
    private readonly IIssueRepository _repository = repository;

    public Task<OperationResult<Issue>> CreateAsync(
        string title,
        string description,
        IssueType type
    ) => CreateAsync(title, description, type, IssuePriority.Medium);

    public async Task<OperationResult<Issue>> CreateAsync(
        string title,
        string description,
        IssueType type,
        IssuePriority priority
    )
    {
        var issue = new Issue(IssueId.New(), title, description, type, priority);
        await _repository.SaveAsync(issue);
        return OperationResult<Issue>.Ok(issue);
    }

    public async Task<OperationResult<Issue>> LoadAsync(IssueId issueId)
    {
        var issue = await _repository.GetByIdAsync(issueId);
        return issue is null
            ? OperationResult<Issue>.Fail("issue_not_found", $"Issue {issueId} was not found.")
            : OperationResult<Issue>.Ok(issue);
    }

    public async Task<OperationResult<Issue>> AssignAsync(IssueId issueId, UserId assigneeId)
    {
        var loadResult = await LoadAsync(issueId);
        if (!loadResult.HasValue || loadResult.Value is null)
        {
            return loadResult;
        }

        loadResult.Value.AssignTo(assigneeId);
        await _repository.SaveAsync(loadResult.Value);
        return loadResult;
    }

    public async Task<OperationResult<Issue>> PlanSprintAsync(IssueId issueId, string sprintKey)
    {
        var loadResult = await LoadAsync(issueId);
        if (!loadResult.HasValue || loadResult.Value is null)
        {
            return loadResult;
        }

        loadResult.Value.PlanForSprint(sprintKey);
        await _repository.SaveAsync(loadResult.Value);
        return loadResult;
    }

    public async Task<OperationResult<Issue>> AddCommentAsync(
        IssueId issueId,
        UserId authorId,
        string message
    )
    {
        var loadResult = await LoadAsync(issueId);
        if (!loadResult.HasValue || loadResult.Value is null)
        {
            return loadResult;
        }

        return await AddCommentAsync(loadResult.Value, authorId, message);
    }

    public async Task<OperationResult<Issue>> AddCommentAsync(
        Issue issue,
        UserId authorId,
        string message
    )
    {
        issue.AddComment(authorId, message);
        await _repository.SaveAsync(issue);
        return OperationResult<Issue>.Ok(issue);
    }

    public async Task<OperationResult<Issue>> TransitionAsync(
        IssueId issueId,
        IssueStatus nextStatus,
        UserId actorId
    )
    {
        var loadResult = await LoadAsync(issueId);
        if (!loadResult.HasValue || loadResult.Value is null)
        {
            return loadResult;
        }

        loadResult.Value.TransitionTo(nextStatus, actorId);
        await _repository.SaveAsync(loadResult.Value);
        return loadResult;
    }

    public Task<PageResult<Issue>> SearchAsync(
        IssueStatus? status,
        IssuePriority? priority,
        UserId? assigneeId,
        string? sprintKey,
        PageRequest page
    ) => _repository.SearchAsync(status, priority, assigneeId, sprintKey, page);
}
