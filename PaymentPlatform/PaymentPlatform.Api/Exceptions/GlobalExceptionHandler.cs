using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PaymentPlatform.Application.Exceptions;

namespace PaymentPlatform.Api.Exceptions;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Unhandled exception occurred");
        
        var (statusCode, title, detail) = exception switch
        {
            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Not Found",
                exception.Message),

            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                exception.Message),

            InvalidOperationException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                exception.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode =
            statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        var written = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
        
        if (!written)
        {
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);
        }

        return true;
    }
}