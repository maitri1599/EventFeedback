using FeedbackApp.Models;
using FeedbackApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FeedbackApp.Pages.Admin.Events;

public class CreateModel : PageModel
{
    private readonly IEventService _eventService;

    public CreateModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    [BindProperty]
    public Event Event { get; set; } = new() { EventDate = DateTime.Today, IncludeRating = true };

    [BindProperty]
    public List<Question> Questions { get; set; } = new();

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("IsAdmin") != "true")
        {
            return RedirectToPage("/Admin/Login");
        }

        Event.EventDate = DateTime.Today.AddDays(7);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HttpContext.Session.GetString("IsAdmin") != "true")
        {
            return RedirectToPage("/Admin/Login");
        }

        // Remove validation for optional fields
        ModelState.Remove("Event.Description");

        // Remove validation for question collection (we validate manually)
        foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Questions")).ToList())
        {
            ModelState.Remove(key);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Filter out empty questions and validate
        var validQuestions = Questions
            .Where(q => !string.IsNullOrWhiteSpace(q.Text))
            .ToList();

        // Validate choice questions have options
        foreach (var q in validQuestions.Where(q =>
            q.Type == QuestionType.SingleChoice || q.Type == QuestionType.MultipleChoice))
        {
            if (string.IsNullOrWhiteSpace(q.Options))
            {
                ModelState.AddModelError(string.Empty, $"Question '{q.Text}' requires options");
                return Page();
            }
        }

        Event.Questions = validQuestions;
        await _eventService.CreateEventAsync(Event);

        return RedirectToPage("/Admin/Index");
    }
}
