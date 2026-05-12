using System.ComponentModel.DataAnnotations;

namespace FeedbackApp.Validation;

/// <summary>
/// Custom Validation Attributes
/// INTERVIEW CONCEPT: Custom Validation + Attributes
///
/// This demonstrates:
/// 1. Creating custom validation attributes
/// 2. Inheritance from ValidationAttribute
/// 3. Server-side validation
/// 4. Reusable validation logic
/// </summary>

/// <summary>
/// Validates that a date is in the future.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FutureDateAttribute : ValidationAttribute
{
    public bool AllowToday { get; set; } = true;

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is DateTime date)
        {
            var compareDate = AllowToday ? DateTime.Today : DateTime.Today.AddDays(1);

            if (date.Date < compareDate)
            {
                return new ValidationResult(
                    ErrorMessage ?? "Date must be in the future.",
                    new[] { context.MemberName! });
            }
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validates that a string doesn't contain profanity (basic example).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class NoProfanityAttribute : ValidationAttribute
{
    private static readonly string[] BlockedWords = { "spam", "test123" };

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is string text && !string.IsNullOrEmpty(text))
        {
            var lowerText = text.ToLowerInvariant();

            foreach (var word in BlockedWords)
            {
                if (lowerText.Contains(word))
                {
                    return new ValidationResult(
                        ErrorMessage ?? "Content contains inappropriate words.",
                        new[] { context.MemberName! });
                }
            }
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validates that at least one property in a group has a value.
/// INTERVIEW CONCEPT: Cross-property validation
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AtLeastOneRequiredAttribute : ValidationAttribute
{
    public string[] PropertyNames { get; }

    public AtLeastOneRequiredAttribute(params string[] propertyNames)
    {
        PropertyNames = propertyNames;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value == null) return ValidationResult.Success;

        var type = value.GetType();
        var hasValue = false;

        foreach (var propName in PropertyNames)
        {
            var prop = type.GetProperty(propName);
            var propValue = prop?.GetValue(value);

            if (propValue != null && !string.IsNullOrWhiteSpace(propValue.ToString()))
            {
                hasValue = true;
                break;
            }
        }

        if (!hasValue)
        {
            return new ValidationResult(
                ErrorMessage ?? $"At least one of {string.Join(", ", PropertyNames)} is required.");
        }

        return ValidationResult.Success;
    }
}
