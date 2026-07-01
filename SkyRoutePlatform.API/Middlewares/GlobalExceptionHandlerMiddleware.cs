using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SkyRoutePlatform.API.Middlewares;

/// <summary>
/// Global exception handler using the built-in IExceptionHandler interface.
/// Converts unhandled exceptions to RFC 7807 Problem Details responses.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    /// <summary>
    /// Creates a new instance of GlobalExceptionHandler.
    /// </summary>
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    /// <summary>
    /// Handles exceptions and converts them to Problem Details responses.
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        ProblemDetails problemDetails = new()
        {
            Detail = exception.Message,
            Status = GetStatusCode(exception)
        };

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }

    /// <summary>
    /// Determines the HTTP status code based on exception type.
    /// </summary>
    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException or ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException or FileNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
