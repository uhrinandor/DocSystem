using DocSystem.Errors;
using DocSystem.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Npgsql;
using Npgsql.PostgresTypes;

namespace DocSystem.Filters;

public class ApiExceptionFilter(
    ILogger<ApiExceptionFilter> logger,
    IHostEnvironment environment) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is DomainException domainException)
        {
            LogDomainException(context.HttpContext, domainException);
            WriteResponse(context, domainException.Error);
            return;
        }

        if (TryMapDatabaseException(context.Exception, out var databaseError))
        {
            logger.LogWarning(
                context.Exception,
                "Database exception. TraceId: {TraceId}; Code: {Code}; LogMessage: {LogMessage}; LogDetails: {@LogDetails}",
                context.HttpContext.TraceIdentifier,
                databaseError.Code,
                databaseError.LogMessage ?? databaseError.Message,
                databaseError.LogDetails);

            WriteResponse(context, databaseError);
            return;
        }

        var error = ErrorResponseFactory.CreateUnhandled(environment.IsDevelopment());

        logger.LogError(
            context.Exception,
            "Unhandled controller exception. TraceId: {TraceId}",
            context.HttpContext.TraceIdentifier);

        WriteResponse(context, error);
    }

    private void WriteResponse(ExceptionContext context, DomainError error)
    {
        context.Result = new ObjectResult(ErrorResponseFactory.Create(context.HttpContext, error))
        {
            StatusCode = error.StatusCode
        };
        context.ExceptionHandled = true;
    }

    private void LogDomainException(HttpContext httpContext, DomainException exception)
    {
        logger.LogWarning(
            exception,
            "Domain exception. TraceId: {TraceId}; Code: {Code}; LogMessage: {LogMessage}; LogDetails: {@LogDetails}",
            httpContext.TraceIdentifier,
            exception.Error.Code,
            exception.Error.LogMessage ?? exception.Message,
            exception.Error.LogDetails);
    }

    private static bool TryMapDatabaseException(Exception exception, out DomainError error)
    {
        if (exception is DbUpdateException dbUpdateException &&
            dbUpdateException.InnerException is PostgresException postgresException)
        {
            if (postgresException.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                error = MapUniqueViolation(postgresException);
                return true;
            }

            error = new DomainError
            {
                Code = "database.update_failed",
                Message = "The database update could not be completed.",
                StatusCode = StatusCodes.Status500InternalServerError,
                LogMessage = "Database update failed.",
                LogDetails = new
                {
                    postgresException.SqlState,
                    postgresException.TableName,
                    postgresException.ConstraintName
                }
            };
            return true;
        }

        error = default!;
        return false;
    }

    private static DomainError MapUniqueViolation(PostgresException exception)
    {
        return exception.ConstraintName switch
        {
            "IX_Iktatokonyvek_Kod_Evszam" => new DomainError
            {
                Code = "iktatokonyv.duplicate_code_year",
                Message = "An iktatokonyv with this code and year already exists.",
                StatusCode = StatusCodes.Status409Conflict,
                LogMessage = "Unique constraint violation while creating iktatokonyv.",
                LogDetails = new
                {
                    exception.TableName,
                    exception.ConstraintName
                }
            },
            _ => new DomainError
            {
                Code = "database.unique_violation",
                Message = "The submitted data conflicts with an existing record.",
                StatusCode = StatusCodes.Status409Conflict,
                LogMessage = "Unhandled unique constraint violation.",
                LogDetails = new
                {
                    exception.TableName,
                    exception.ConstraintName
                }
            }
        };
    }
}
