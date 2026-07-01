namespace SkyRoutePlatform.API.Shared.Responses;

/// <summary>
/// Unified API response wrapper for all endpoints.
/// Standardizes response format with data, status code, message, and timestamp.
/// </summary>
/// <typeparam name="T">The type of response data (must be a class)</typeparam>
public sealed record ApiResponse<T> where T : class
{
    /// <summary>
    /// Creates a new API response.
    /// </summary>
    /// <param name="data">The response data (null for errors)</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="message">Human-readable message</param>
    /// <param name="details">Optional error details</param>
    /// <param name="timestamp">Response timestamp (defaults to UTC now)</param>
    public ApiResponse(T? data, int statusCode, string message, Dictionary<string, string?>? details = null, DateTime timestamp = default)
    {
        Data = data;
        StatusCode = statusCode;
        Message = message;
        Details = details;
        Timestamp = timestamp == default ? DateTime.UtcNow : timestamp;
    }

    /// <summary>
    /// The response data payload.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// HTTP status code (200-599).
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// Human-readable message describing the result.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Optional error details or context.
    /// </summary>
    public Dictionary<string, string?>? Details { get; init; }

    /// <summary>
    /// Response timestamp in UTC.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Indicates if the response represents success (2xx status code).
    /// </summary>
    public bool IsSuccess => StatusCode is >= 200 and < 300;

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <param name="data">The response data</param>
    /// <param name="statusCode">HTTP status code (default 200)</param>
    /// <param name="message">Success message</param>
    /// <returns>Success ApiResponse</returns>
    public static ApiResponse<T> Success(T data, int statusCode = StatusCodes.Status200OK, string message = "Success")
    {
        return new ApiResponse<T>(data, statusCode, message);
    }

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="message">Error message</param>
    /// <param name="details">Optional error details</param>
    /// <returns>Error ApiResponse</returns>
    public static ApiResponse<T> Failure(int statusCode, string message, Dictionary<string, string?>? details = null)
    {
        return new ApiResponse<T>(null, statusCode, message, details);
    }
}

/// <summary>
/// Extension methods for ApiResponse<T>.
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Matches the ApiResponse to either a success or error handler based on IsSuccess.
    /// </summary>
    /// <typeparam name="T">The type of response data</typeparam>
    /// <param name="response">The ApiResponse instance</param>
    /// <param name="success">Function to handle success</param>
    /// <param name="error">Function to handle error</param>
    /// <returns>The result of the matched function</returns>
    public static IResult Match<T>(this ApiResponse<T> response, Func<ApiResponse<T>, IResult> success, Func<ApiResponse<T>, IResult> error) where T : class => response.IsSuccess ? success(response) : error(response);

    /// <summary>
    /// Converts an error ApiResponse to the appropriate IResult based on status code.
    /// Maps error codes to their corresponding HTTP result types.
    /// </summary>
    /// <typeparam name="T">The type of response data</typeparam>
    /// <param name="response">The error ApiResponse to convert</param>
    /// <returns>IResult with appropriate error status code and response body</returns>
    public static IResult ToErrorResult<T>(this ApiResponse<T> response) where T : class
    {
        return response.StatusCode switch
        {
            StatusCodes.Status400BadRequest => Results.BadRequest(response),
            StatusCodes.Status404NotFound => Results.NotFound(response),
            StatusCodes.Status422UnprocessableEntity => Results.UnprocessableEntity(response),
            StatusCodes.Status409Conflict => Results.Conflict(response),
            StatusCodes.Status403Forbidden => Results.Forbid(),
            StatusCodes.Status401Unauthorized => Results.Unauthorized(),
            _ => Results.StatusCode(response.StatusCode)
        };
    }
}

