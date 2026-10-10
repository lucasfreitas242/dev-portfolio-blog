namespace Blog.Api.Infrastructure;

using System.Diagnostics;
using Blog.Application.Exceptions;
using Blog.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var (statusCode, title, detail, type) = exception switch
        {
            UnauthorizedException unauthorized => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                unauthorized.Message,
                "https://tools.ietf.org/html/rfc9110#section-15.5.2"
            ),
            NotFoundException notFound => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                notFound.Message,
                "https://tools.ietf.org/html/rfc9110#section-15.5.5"
            ),
            ValidationException validation => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                validation.Message,
                "https://tools.ietf.org/html/rfc9110#section-15.5.1"
            ),
            ConflictException conflict => (
                StatusCodes.Status409Conflict,
                "Conflict",
                conflict.Message,
                "https://tools.ietf.org/html/rfc9110#section-15.5.10"
            ),
            DomainRuleViolationException domainRule => (
                StatusCodes.Status422UnprocessableEntity,
                "Unprocessable Entity",
                domainRule.Message,
                "https://tools.ietf.org/html/rfc9110#section-15.5.21"
            ),
            DomainException domainEx => (
                StatusCodes.Status400BadRequest,
                "Domain Rule Violation",
                domainEx.Message,
                "https://tools.ietf.org/html/rfc9110#section-15.5.1"
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                "An unexpected internal error occurred on the server.",
                "https://tools.ietf.org/html/rfc9110#section-15.6.1"
            )
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception occurred: {Message}. TraceId: {TraceId}", exception.Message, traceId);
        }
        else
        {
            logger.LogWarning("Handled application exception: {Title} ({StatusCode}) - {Detail}. TraceId: {TraceId}", title, statusCode, detail, traceId);
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = type,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;

        if (exception is ValidationException validationEx && validationEx.Errors.Count > 0)
        {
            problemDetails.Extensions["errors"] = validationEx.Errors;
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
