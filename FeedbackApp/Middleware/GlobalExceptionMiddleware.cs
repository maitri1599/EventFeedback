using System.Text.Json;
using FeedbackApp.Exceptions;

namespace FeedbackApp.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.TraceIdentifier;

        _logger.LogError(exception,
            "An error occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
            correlationId,
            context.Request.Path,
            context.Request.Method);

        var response = CreateErrorResponse(exception, correlationId);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = response.Status;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private ProblemDetails CreateErrorResponse(Exception exception, string correlationId)
    {
        var response = exception switch
        {
            NotFoundException ex => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Resource Not Found",
                Status = ex.StatusCode,
                Detail = ex.Message,
                ErrorCode = ex.ErrorCode
            },
            ValidationException ex => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Validation Error",
                Status = ex.StatusCode,
                Detail = ex.Message,
                ErrorCode = ex.ErrorCode,
                Errors = ex.Errors
            },
            UnauthorizedException ex => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Status = ex.StatusCode,
                Detail = ex.Message,
                ErrorCode = ex.ErrorCode
            },
            ForbiddenException ex => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Status = ex.StatusCode,
                Detail = ex.Message,
                ErrorCode = ex.ErrorCode
            },
            BusinessRuleException ex => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                Title = "Business Rule Violation",
                Status = ex.StatusCode,
                Detail = ex.Message,
                ErrorCode = ex.ErrorCode
            },
            ArgumentNullException ex => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Invalid Argument",
                Status = 400,
                Detail = _environment.IsDevelopment() ? ex.Message : "A required parameter was missing.",
                ErrorCode = "INVALID_ARGUMENT"
            },
            _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = 500,
                Detail = _environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.",
                ErrorCode = "INTERNAL_ERROR"
            }
        };

        response.CorrelationId = correlationId;

        if (_environment.IsDevelopment() && response.Status == 500)
            response.StackTrace = exception.StackTrace;

        return response;
    }
}

public class ProblemDetails
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
