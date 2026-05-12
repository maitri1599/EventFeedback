using System.ComponentModel.DataAnnotations;

namespace FeedbackApp.Models;

public class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Question text is required")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "Question must be 3-500 characters")]
    public string Text { get; set; } = string.Empty;

    public QuestionType Type { get; set; } = QuestionType.Text;

    public bool IsRequired { get; set; } = true;

    // For multiple choice questions - comma separated options
    public string? Options { get; set; }

    public int Order { get; set; }

    // Helper to get options as list
    public List<string> GetOptionsList()
    {
        if (string.IsNullOrWhiteSpace(Options)) return new List<string>();
        return Options.Split(',', StringSplitOptions.RemoveEmptyEntries)
                     .Select(o => o.Trim())
                     .Where(o => !string.IsNullOrEmpty(o))
                     .ToList();
    }
}

public enum QuestionType
{
    Text,           // Short text answer
    TextArea,       // Long text answer
    SingleChoice,   // Radio buttons
    MultipleChoice, // Checkboxes
    YesNo           // Yes/No toggle
}
