using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FeedbackApp.Exceptions;

namespace FeedbackApp.Filters;

public class ApiExceptionFilter : IAsyncExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;
    private readonly IHostEnvironment _environment;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public Task OnExceptionAsync(ExceptionContext context)
    {
        var exception = context.Exception;
        var correlationId = context.HttpContext.TraceIdentifier;

        _logger.LogError(exception,
            "Exception caught by filter. CorrelationId: {CorrelationId}, Path: {Path}",
            correlationId,
            context.HttpContext.Request.Path);

        var problemDetails = CreateProblemDetails(exception, correlationId);

        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status
        };

        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }

    private ApiProblemDetails CreateProblemDetails(Exception exception, string correlationId)
    {
        return exception switch
        {
            NotFoundException ex => new ApiProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Not Found",
                Status = ex.StatusCode,
                Detail = ex.Message,
                ErrorCode = ex.ErrorCode,
                CorrelationId = correlationId
            },
            ValidationException ex => new ApiProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Validation Error",
                Status = ex.StatusCode,
                Detail = ex.Message,
                ErrorCode = ex.ErrorCode,
                CorrelationId = correlationId,
                Errors = ex.Errors
            },
            UnauthorizedException ex => new ApiProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Status = ex.StatusCode,
                Detail = ex.Message,
                ErrorCode = ex.ErrorCode,
                CorrelationId = correlationId
            },
            ForbiddenException ex => new ApiProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Status = ex.StatusCode,
                Detail = ex.Message,
                ErrorCode = ex.ErrorCode,
                CorrelationId = correlationId
            },
            _ => new ApiProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = 500,
                Detail = _environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred.",
                ErrorCode = "INTERNAL_ERROR",
                CorrelationId = correlationId
            }
        };
    }
}

public class ApiProblemDetails
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public IDictionary<string, string[]>? Errors { get; set; }
}
