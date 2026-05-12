using FeedbackApp.Models;
using FeedbackApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FeedbackApp.Pages.Admin.Events;

public class EditModel : PageModel
{
    private readonly IEventService _eventService;

    public EditModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    [BindProperty]
    public Event? Event { get; set; }

    [BindProperty]
    public List<Question> Questions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        if (HttpContext.Session.GetString("IsAdmin") != "true")
        {
            return RedirectToPage("/Admin/Login");
        }

        Event = await _eventService.GetEventByIdAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (HttpContext.Session.GetString("IsAdmin") != "true")
        {
            return RedirectToPage("/Admin/Login");
        }

        // Remove validation for optional fields
        ModelState.Remove("Event.Description");

        // Remove validation for question collection
        foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Questions")).ToList())
        {
            ModelState.Remove(key);
        }

        if (!ModelState.IsValid || Event == null)
        {
            return Page();
        }

        // Get existing event to preserve ID
        var existingEvent = await _eventService.GetEventByIdAsync(id);
        if (existingEvent == null)
        {
            return NotFound();
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
                Event = existingEvent;
                return Page();
            }
        }

        // Assign order to questions and generate IDs for new ones
        for (int i = 0; i < validQuestions.Count; i++)
        {
            if (validQuestions[i].Id == Guid.Empty)
            {
                validQuestions[i].Id = Guid.NewGuid();
            }
            validQuestions[i].Order = i;
        }

        // Update event
        Event.Id = id;
        Event.CreatedAt = existingEvent.CreatedAt;
        Event.Questions = validQuestions;

        await _eventService.UpdateEventAsync(Event);

        return RedirectToPage("/Admin/Index");
    }
}
