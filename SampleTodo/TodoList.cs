namespace SampleTodo;

public class TodoList(string name)
{
    public string Name { get; } = name;

    private readonly List<TodoItem> _items = [];

    public IReadOnlyList<TodoItem> Items => _items;

    public int Count => _items.Count;

    public int PendingCount => _items.Count(i => !i.Completed);

    public void Add(TodoItem item) => _items.Add(item);

    public void Add(string title) => _items.Add(new TodoItem(title));

    public void Add(string title, Priority priority) => _items.Add(new TodoItem(title, Priority: priority));

    public TodoItem? FindByTitle(string title) => _items.FirstOrDefault(i => i.Title == title);

    public bool HasPending() => _items.Any(i => !i.Completed);
}
