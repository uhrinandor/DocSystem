namespace DocSystem.Errors;

public class DomainError
{
    public string Code { get; init; } = "domain_error";
    public string Message { get; init; } = "A domain error occurred.";
    public int StatusCode { get; init; } = StatusCodes.Status400BadRequest;
    public object? Details { get; init; }
    public string? LogMessage { get; init; }
    public object? LogDetails { get; init; }
}
