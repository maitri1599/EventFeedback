using System.ComponentModel.DataAnnotations;

namespace FeedbackApp.Models;

public class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Event name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Event name must be 3-100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Event date is required")]
    [DataType(DataType.Date)]
    public DateTime EventDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // New: Whether to include star rating in feedback form
    public bool IncludeRating { get; set; } = true;

    // New: Custom questions for this event
    public List<Question> Questions { get; set; } = new();

    // Computed property - not stored
    public EventStatus Status => EventDate.Date >= DateTime.Today ? EventStatus.Upcoming : EventStatus.Past;

    public bool CanSubmitFeedback => Status == EventStatus.Upcoming || EventDate.Date == DateTime.Today;

    // Generate shareable link token (simple approach using event ID)
    public string ShareToken => Id.ToString("N")[..12]; // First 12 chars of GUID without dashes
}

public enum EventStatus
{
    Upcoming,
    Past
}
