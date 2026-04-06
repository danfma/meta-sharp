using SampleIssueTracker.Issues.Domain;

namespace SampleIssueTracker.Planning.Domain;

public class Sprint(string key, string name, DateOnly startDate, DateOnly endDate)
{
    public string Key { get; } = key;

    public string Name { get; private set; } = name;

    public DateOnly StartDate { get; private set; } = startDate;

    public DateOnly EndDate { get; private set; } = endDate;

    private readonly HashSet<IssueId> _plannedIssues = [];

    public IReadOnlyCollection<IssueId> PlannedIssues => _plannedIssues;

    public int PlannedCount => _plannedIssues.Count;

    public int DurationDays => EndDate.DayNumber - StartDate.DayNumber + 1;

    public bool IsActiveOn(DateOnly date) => date >= StartDate && date <= EndDate;

    public void Rename(string newName) => Name = newName;

    public void Reschedule(DateOnly newStartDate, DateOnly newEndDate)
    {
        StartDate = newStartDate;
        EndDate = newEndDate;
    }

    public void Plan(IssueId issueId) => _plannedIssues.Add(issueId);

    public void Unplan(IssueId issueId) => _plannedIssues.Remove(issueId);
}
