namespace SampleTodo;

public record TodoItem(string Title, bool Completed = false, Priority Priority = Priority.Medium)
{
    public TodoItem ToggleCompleted() => this with { Completed = !Completed };

    public TodoItem SetPriority(Priority priority) => this with { Priority = priority };

    public override string ToString() => $"[{(Completed ? "x" : " ")}] {Title} ({Priority})";
}
