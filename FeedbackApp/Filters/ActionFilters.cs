using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FeedbackApp.Filters;

public class LoggingActionFilter : IAsyncActionFilter
{
    private readonly ILogger<LoggingActionFilter> _logger;

    public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var actionName = context.ActionDescriptor.DisplayName;
        var controllerName = context.Controller.GetType().Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Executing {Controller}.{Action} with arguments: {@Arguments}",
            controllerName,
            actionName,
            context.ActionArguments);

        var resultContext = await next();

        stopwatch.Stop();

        if (resultContext.Exception != null)
        {
            _logger.LogError(
                "{Controller}.{Action} failed after {ElapsedMs}ms: {Error}",
                controllerName,
                actionName,
                stopwatch.ElapsedMilliseconds,
                resultContext.Exception.Message);
        }
        else
        {
            _logger.LogInformation(
                "{Controller}.{Action} completed in {ElapsedMs}ms",
                controllerName,
                actionName,
                stopwatch.ElapsedMilliseconds);
        }
    }
}

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred.",
                Instance = context.HttpContext.Request.Path
            };

            context.Result = new BadRequestObjectResult(problemDetails);
        }
    }
}

public class PerformanceFilter : IAsyncActionFilter
{
    private readonly ILogger<PerformanceFilter> _logger;
    private readonly int _thresholdMs;

    public PerformanceFilter(ILogger<PerformanceFilter> logger, int thresholdMs = 500)
    {
        _logger = logger;
        _thresholdMs = thresholdMs;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();

        await next();

        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > _thresholdMs)
        {
            _logger.LogWarning(
                "SLOW REQUEST: {Method} {Path} took {ElapsedMs}ms (threshold: {Threshold}ms)",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path,
                stopwatch.ElapsedMilliseconds,
                _thresholdMs);
        }
    }
}

public class IdempotencyFilter : IAsyncActionFilter
{
    private static readonly Dictionary<string, DateTime> _processedRequests = new();
    private static readonly object _lock = new();
    private readonly TimeSpan _expirationTime = TimeSpan.FromMinutes(5);

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKey))
        {
            await next();
            return;
        }

        var key = idempotencyKey.ToString();

        lock (_lock)
        {
            var expiredKeys = _processedRequests
                .Where(x => DateTime.UtcNow - x.Value > _expirationTime)
                .Select(x => x.Key)
                .ToList();

            foreach (var expiredKey in expiredKeys)
                _processedRequests.Remove(expiredKey);

            if (_processedRequests.ContainsKey(key))
            {
                context.Result = new ConflictObjectResult(new
                {
                    error = "Duplicate request",
                    message = "This request has already been processed",
                    idempotencyKey = key
                });
                return;
            }

            _processedRequests[key] = DateTime.UtcNow;
        }

        await next();
    }
}
