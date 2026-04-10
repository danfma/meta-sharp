using SampleIssueTracker.SharedKernel;

namespace SampleIssueTracker.Issues.Domain;

public class Issue(
    IssueId id,
    string title,
    string description,
    IssueType type,
    IssuePriority priority = IssuePriority.Medium
)
{
    public IssueId Id { get; } = id;

    public string Title { get; private set; } = title;

    public string Description { get; private set; } = description;

    public IssueType Type { get; } = type;

    public IssuePriority Priority { get; private set; } = priority;

    public IssueStatus Status { get; private set; } = IssueStatus.Backlog;

    public UserId? AssigneeId { get; private set; }

    public string? SprintKey { get; private set; }

    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private readonly List<Comment> _comments = [];

    public IReadOnlyList<Comment> Comments => _comments;

    public bool IsClosed => Status is IssueStatus.Done or IssueStatus.Cancelled;

    public int CommentCount => _comments.Count;

    public string Lane => IssueWorkflow.DescribeLane(this);

    public void Rename(string newTitle)
    {
        Title = newTitle;
        Touch(DateTimeOffset.UtcNow);
    }

    public void Describe(string newDescription)
    {
        Description = newDescription;
        Touch(DateTimeOffset.UtcNow);
    }

    public void ChangePriority(IssuePriority newPriority)
    {
        Priority = newPriority;
        Touch(DateTimeOffset.UtcNow);
    }

    public void AssignTo(UserId assigneeId)
    {
        AssigneeId = assigneeId;
        Touch(DateTimeOffset.UtcNow);
    }

    public void Unassign()
    {
        AssigneeId = null;
        Touch(DateTimeOffset.UtcNow);
    }

    public void PlanForSprint(string sprintKey)
    {
        SprintKey = sprintKey;
        Touch(DateTimeOffset.UtcNow);
    }

    public void RemoveFromSprint()
    {
        SprintKey = null;
        Touch(DateTimeOffset.UtcNow);
    }

    public void AddComment(UserId authorId, string message) =>
        AddComment(authorId, message, DateTimeOffset.UtcNow);

    public void AddComment(UserId authorId, string message, DateTimeOffset createdAt)
    {
        _comments.Add(new Comment(authorId, message, createdAt));
        Touch(createdAt);
    }

    public void TransitionTo(IssueStatus nextStatus, UserId actorId) =>
        TransitionTo(nextStatus, actorId, DateTimeOffset.UtcNow);

    public void TransitionTo(IssueStatus nextStatus, UserId actorId, DateTimeOffset changedAt)
    {
        var previousStatus = Status;

        if (!IssueWorkflow.CanTransition(previousStatus, nextStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition issue from {previousStatus} to {nextStatus}."
            );
        }

        Status = nextStatus;
        _comments.Add(
            Comment.System(
                $"Status changed from {previousStatus} to {nextStatus} by {actorId}.",
                changedAt
            )
        );
        Touch(changedAt);
    }

    private void Touch(DateTimeOffset updatedAt) => UpdatedAt = updatedAt;
}
