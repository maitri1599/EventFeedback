using FeedbackApp.Models;
using FeedbackApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FeedbackApp.Pages.Feedback;

public class SubmitModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IFeedbackService _feedbackService;

    public SubmitModel(IEventService eventService, IFeedbackService feedbackService)
    {
        _eventService = eventService;
        _feedbackService = feedbackService;
    }

    public Event? Event { get; set; }

    [BindProperty]
    public Models.Feedback Feedback { get; set; } = new();

    [BindProperty]
    public Dictionary<string, string> Answers { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string token)
    {
        Event = await _eventService.GetEventByTokenAsync(token);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string token)
    {
        Event = await _eventService.GetEventByTokenAsync(token);

        if (Event == null)
        {
            return Page();
        }

        if (!Event.CanSubmitFeedback)
        {
            ModelState.AddModelError(string.Empty, "Feedback submission is closed for this event.");
            return Page();
        }

        // Clear validation for optional fields
        ModelState.Remove("Feedback.Name");
        ModelState.Remove("Feedback.Comments");
        ModelState.Remove("Feedback.Rating");

        // Validate rating if required
        if (Event.IncludeRating && (!Feedback.Rating.HasValue || Feedback.Rating < 1))
        {
            ModelState.AddModelError("Feedback.Rating", "Please provide a rating");
        }

        // Validate required custom questions
        foreach (var question in Event.Questions.Where(q => q.IsRequired))
        {
            var key = question.Id.ToString();
            if (!Answers.ContainsKey(key) || string.IsNullOrWhiteSpace(Answers[key]))
            {
                ModelState.AddModelError($"Answers[{key}]", $"This question is required");
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Process multiple choice answers (combine checkbox values)
        var processedAnswers = new Dictionary<string, string>();
        foreach (var answer in Answers)
        {
            processedAnswers[answer.Key] = answer.Value;
        }

        // Get checkbox values from form (they come as multiple values)
        foreach (var question in Event.Questions.Where(q => q.Type == QuestionType.MultipleChoice))
        {
            var key = question.Id.ToString();
            var values = Request.Form[$"Answers[{key}]"];
            if (values.Count > 0)
            {
                processedAnswers[key] = string.Join(", ", values.ToArray());
            }
        }

        Feedback.EventId = Event.Id;
        Feedback.Answers = processedAnswers;

        // If rating not included, set to null
        if (!Event.IncludeRating)
        {
            Feedback.Rating = null;
        }

        await _feedbackService.SubmitFeedbackAsync(Feedback);

        TempData["Success"] = "Your feedback has been submitted successfully!";
        return RedirectToPage(new { token });
    }
}
