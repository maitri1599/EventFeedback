namespace FeedbackApp.Exceptions;

public abstract class AppException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    protected AppException(string message, string errorCode, int statusCode = 500)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string resource, object key)
        : base($"{resource} with identifier '{key}' was not found.", "RESOURCE_NOT_FOUND", 404) { }

    public NotFoundException(string message)
        : base(message, "RESOURCE_NOT_FOUND", 404) { }
}

public class ValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message)
        : base(message, "VALIDATION_ERROR", 400)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", "VALIDATION_ERROR", 400)
    {
        Errors = errors;
    }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "You are not authorized to perform this action.")
        : base(message, "UNAUTHORIZED", 401) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You don't have permission to access this resource.")
        : base(message, "FORBIDDEN", 403) { }
}

public class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message, "CONFLICT", 409) { }
}

public class BusinessRuleException : AppException
{
    public BusinessRuleException(string message)
        : base(message, "BUSINESS_RULE_VIOLATION", 422) { }
}
