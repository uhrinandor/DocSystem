namespace DocSystem.Errors;

public static class ErrorResponseFactory
{
    public static ApiErrorResponse Create(HttpContext httpContext, DomainError error)
    {
        return new ApiErrorResponse
        {
            TraceId = httpContext.TraceIdentifier,
            Error = new ApiErrorBody
            {
                Code = error.Code,
                Message = error.Message,
                Details = error.Details
            }
        };
    }

    public static DomainError CreateUnhandled(bool includeExceptionDetails)
    {
        return new DomainError
        {
            Code = "internal_server_error",
            Message = "An unexpected error occurred.",
            StatusCode = StatusCodes.Status500InternalServerError,
            Details = includeExceptionDetails
                ? new
                {
                    note = "See server logs for additional details."
                }
                : null,
            LogMessage = "Unhandled exception reached the API boundary."
        };
    }
}
