namespace DocSystem.Errors;

public class ApiErrorResponse
{
    public string TraceId { get; init; } = string.Empty;
    public ApiErrorBody Error { get; init; } = new();
}

public class ApiErrorBody
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public object? Details { get; init; }
}
