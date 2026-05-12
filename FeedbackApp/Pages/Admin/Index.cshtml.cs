using FeedbackApp.Models;
using FeedbackApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FeedbackApp.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IFeedbackService _feedbackService;
    private Dictionary<Guid, double> _avgRatings = new();
    private Dictionary<Guid, int> _feedbackCounts = new();

    public IndexModel(IEventService eventService, IFeedbackService feedbackService)
    {
        _eventService = eventService;
        _feedbackService = feedbackService;
    }

    public IEnumerable<Event> Events { get; set; } = Enumerable.Empty<Event>();
    public int TotalEvents { get; set; }
    public int UpcomingEvents { get; set; }
    public int TotalFeedbacks { get; set; }
    public double OverallRating { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Check admin authentication
        if (HttpContext.Session.GetString("IsAdmin") != "true")
        {
            return RedirectToPage("/Admin/Login");
        }

        Events = await _eventService.GetAllEventsAsync();
        TotalEvents = Events.Count();
        UpcomingEvents = Events.Count(e => e.Status == EventStatus.Upcoming);

        // Calculate stats
        double totalRating = 0;
        int ratingCount = 0;

        foreach (var evt in Events)
        {
            var avgRating = await _feedbackService.GetAverageRatingAsync(evt.Id);
            var count = await _feedbackService.GetFeedbackCountAsync(evt.Id);

            _avgRatings[evt.Id] = avgRating;
            _feedbackCounts[evt.Id] = count;

            TotalFeedbacks += count;
            if (avgRating > 0)
            {
                totalRating += avgRating;
                ratingCount++;
            }
        }

        OverallRating = ratingCount > 0 ? Math.Round(totalRating / ratingCount, 1) : 0;

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteEventAsync(Guid id)
    {
        if (HttpContext.Session.GetString("IsAdmin") != "true")
        {
            return RedirectToPage("/Admin/Login");
        }

        await _eventService.DeleteEventAsync(id);
        return RedirectToPage();
    }

    public double GetAverageRating(Guid eventId) =>
        _avgRatings.TryGetValue(eventId, out var rating) ? rating : 0;

    public int GetFeedbackCount(Guid eventId) =>
        _feedbackCounts.TryGetValue(eventId, out var count) ? count : 0;
}
