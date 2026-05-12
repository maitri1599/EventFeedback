using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FeedbackApp.Filters;
using FeedbackApp.Services.Interfaces;
using FeedbackApp.Models;
using FeedbackApp.Common;

namespace FeedbackApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[TypeFilter(typeof(ApiExceptionFilter))]
public class FeedbackApiController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;
    private readonly IEventService _eventService;
    private readonly ILogger<FeedbackApiController> _logger;

    public FeedbackApiController(
        IFeedbackService feedbackService,
        IEventService eventService,
        ILogger<FeedbackApiController> logger)
    {
        _feedbackService = feedbackService;
        _eventService = eventService;
        _logger = logger;
    }

    [HttpGet("event/{eventId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Feedback>>> GetByEvent(Guid eventId)
    {
        var eventEntity = await _eventService.GetEventByIdAsync(eventId);
        if (eventEntity == null)
            return NotFound(new { error = "Event not found", eventId });

        var feedback = await _feedbackService.GetFeedbacksForEventAsync(eventId);
        return Ok(feedback);
    }

    [HttpPost]
    [AllowAnonymous]
    [ServiceFilter(typeof(IdempotencyFilter))]
    [ValidateModel]
    public async Task<ActionResult<Feedback>> Create([FromBody] CreateFeedbackRequest request)
    {
        var eventEntity = await _eventService.GetEventByIdAsync(request.EventId);
        if (eventEntity == null)
            return NotFound(new { error = "Event not found" });

        var feedback = new Feedback
        {
            EventId = request.EventId,
            Name = request.Name,
            Rating = request.Rating,
            Comments = request.Comments,
            Answers = request.Answers ?? new Dictionary<string, string>()
        };

        var created = await _feedbackService.SubmitFeedbackAsync(feedback);

        _logger.LogInformation("Feedback {FeedbackId} created for event {EventId}",
            created.Id, created.EventId);

        return Created($"/api/feedbackapi/event/{created.EventId}", created);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _feedbackService.DeleteFeedbackAsync(id);

        _logger.LogInformation("Feedback {FeedbackId} deleted", id);

        return NoContent();
    }

    [HttpGet("stats/{eventId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FeedbackStats>> GetStats(Guid eventId)
    {
        var eventEntity = await _eventService.GetEventByIdAsync(eventId);
        if (eventEntity == null)
            return NotFound(new { error = "Event not found" });

        var feedbackList = (await _feedbackService.GetFeedbacksForEventAsync(eventId)).ToList();
        var averageRating = await _feedbackService.GetAverageRatingAsync(eventId);
        var feedbackCount = await _feedbackService.GetFeedbackCountAsync(eventId);
        var ratingsOnly = feedbackList.Where(f => f.Rating.HasValue).Select(f => f.Rating!.Value).ToList();

        var stats = new FeedbackStats
        {
            EventId = eventId,
            EventName = eventEntity.Name,
            TotalFeedback = feedbackCount,
            AverageRating = averageRating > 0 ? averageRating : null,
            RatingDistribution = ratingsOnly.GroupBy(r => r).ToDictionary(g => g.Key, g => g.Count()),
            LatestSubmission = feedbackList.MaxBy(f => f.SubmittedAt)?.SubmittedAt
        };

        return Ok(stats);
    }
}

public class CreateFeedbackRequest
{
    public Guid EventId { get; set; }
    public string? Name { get; set; }
    public int? Rating { get; set; }
    public string? Comments { get; set; }
    public Dictionary<string, string>? Answers { get; set; }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

public class FeedbackStats
{
    public Guid EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int TotalFeedback { get; set; }
    public double? AverageRating { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
    public DateTime? LatestSubmission { get; set; }
}
