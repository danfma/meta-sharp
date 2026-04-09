namespace SampleIssueTracker.Issues.Domain;

public static class IssueWorkflow
{
    public static IReadOnlyList<IssueStatus> GetAllowedTransitions(IssueStatus currentStatus) =>
        currentStatus switch
        {
            IssueStatus.Backlog => [IssueStatus.Ready, IssueStatus.Cancelled],
            IssueStatus.Ready => [IssueStatus.InProgress, IssueStatus.Cancelled],
            IssueStatus.InProgress => [IssueStatus.InReview, IssueStatus.Backlog, IssueStatus.Cancelled],
            IssueStatus.InReview => [IssueStatus.Done, IssueStatus.InProgress, IssueStatus.Cancelled],
            _ => [],
        };

    public static bool CanTransition(IssueStatus currentStatus, IssueStatus nextStatus) =>
        GetAllowedTransitions(currentStatus).Contains(nextStatus);

    public static string DescribeLane(Issue issue) =>
        issue switch
        {
            { Status: IssueStatus.Backlog } => "triage",
            { Status: IssueStatus.Ready, Priority: IssuePriority.Urgent } => "expedite",
            { Status: IssueStatus.Ready } => "ready",
            { Status: IssueStatus.InProgress } => "building",
            { Status: IssueStatus.InReview } => "review",
            { Status: IssueStatus.Done } => "done",
            _ => "cancelled",
        };
}
