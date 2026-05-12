using FeedbackApp.Models;
using FeedbackApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FeedbackApp.Pages.Admin.Events;

public class DetailsModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IFeedbackService _feedbackService;

    public DetailsModel(IEventService eventService, IFeedbackService feedbackService)
    {
        _eventService = eventService;
        _feedbackService = feedbackService;
    }

    public Event? Event { get; set; }
    public IEnumerable<Models.Feedback> Feedbacks { get; set; } = Enumerable.Empty<Models.Feedback>();
    public double AverageRating { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        if (HttpContext.Session.GetString("IsAdmin") != "true")
        {
            return RedirectToPage("/Admin/Login");
        }

        Event = await _eventService.GetEventByIdAsync(id);

        if (Event != null)
        {
            Feedbacks = await _feedbackService.GetFeedbacksForEventAsync(id);
            AverageRating = await _feedbackService.GetAverageRatingAsync(id);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteFeedbackAsync(Guid id, Guid feedbackId)
    {
        if (HttpContext.Session.GetString("IsAdmin") != "true")
        {
            return RedirectToPage("/Admin/Login");
        }

        await _feedbackService.DeleteFeedbackAsync(feedbackId);
        TempData["Success"] = "Feedback deleted successfully!";
        return RedirectToPage(new { id });
    }
}
