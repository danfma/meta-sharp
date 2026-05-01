using Metano.Annotations;

namespace SampleIssueTracker.SharedKernel;

[Branded]
public readonly record struct UserId(string Value)
{
    public static UserId New() => new(Guid.NewGuid().ToString("N"));

    public static UserId System() => new("system");

    public override string ToString() => Value;
}
