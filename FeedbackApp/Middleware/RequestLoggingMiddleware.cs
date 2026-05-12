using System.Diagnostics;

namespace FeedbackApp.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        _logger.LogInformation("{Method} {Path} - Started", requestMethod, requestPath);

        try
        {
            await _next(context);

            stopwatch.Stop();

            _logger.LogInformation(
                "{Method} {Path} - Completed in {ElapsedMs}ms with status {StatusCode}",
                requestMethod,
                requestPath,
                stopwatch.ElapsedMilliseconds,
                context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "{Method} {Path} - Failed after {ElapsedMs}ms",
                requestMethod,
                requestPath,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
