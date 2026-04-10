namespace SampleIssueTracker.SharedKernel;

public record OperationResult<T>(
    bool Success,
    T? Value,
    string? ErrorCode = null,
    string? ErrorMessage = null
)
{
    public bool HasValue => Success && Value is not null;

    public static OperationResult<T> Ok(T value) => new(true, value);

    public static OperationResult<T> Fail(string errorCode, string errorMessage) =>
        new(false, default, errorCode, errorMessage);
}
