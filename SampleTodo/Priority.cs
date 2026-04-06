using MetaSharp;

namespace SampleTodo;

[StringEnum]
public enum Priority
{
    [Name("low")] Low,
    [Name("medium")] Medium,
    [Name("high")] High,
}
