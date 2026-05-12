using System.ComponentModel.DataAnnotations;

namespace FeedbackApp.Models;

public class Feedback
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid EventId { get; set; }

    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string? Name { get; set; }

    // Rating is now optional (depends on event settings)
    [Range(0, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int? Rating { get; set; }

    [StringLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters")]
    public string? Comments { get; set; }

    // Answers to custom questions - Dictionary<QuestionId, Answer>
    public Dictionary<string, string> Answers { get; set; } = new();

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Display helper
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "Anonymous" : Name;
}
