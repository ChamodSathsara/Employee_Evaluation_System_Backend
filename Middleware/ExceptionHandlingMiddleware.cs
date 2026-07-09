using System.Net;
using System.Text.Json;
using EmployeeEvaluationSystem.Helpers;

namespace EmployeeEvaluationSystem.Middleware;

/// <summary>
/// Centralized exception handler. Every AppException subclass (NotFoundException,
/// ValidationAppException, UnauthorizedAppException, ForbiddenException, ConflictException)
/// is translated into the standard { success, message } envelope with the matching HTTP
/// status code (400 / 401 / 403 / 404 / 409). Any other unhandled exception is logged and
/// returned as a generic 500 without leaking internal details to the client.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }

        catch (AppException appException)
        {
            _logger.LogWarning(appException, "Handled application exception: {Message}", appException.Message);
            await WriteResponseAsync(context, appException.StatusCode, appException.Message);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
            _logger.LogError("FULL ERROR - Type: {ExceptionType}", ex.GetType().FullName);
            _logger.LogError("FULL ERROR - Message: {Message}", ex.Message);
            _logger.LogError("FULL ERROR - StackTrace: {StackTrace}", ex.StackTrace);

            // Log all inner exceptions
            var innerEx = ex.InnerException;
            int level = 1;
            while (innerEx != null)
            {
                _logger.LogError("INNER EXCEPTION {Level} - Type: {Type}, Message: {Message}", level, innerEx.GetType().FullName, innerEx.Message);
                innerEx = innerEx.InnerException;
                level++;
            }

            await WriteResponseAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.");
        }

    }

    private static async Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse.FailResponse(message);
        var payload = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(payload);
    }
}
