using FeedbackApp.Models;
using FeedbackApp.Services.Interfaces;

namespace FeedbackApp.Services;

public class FeedbackService : IFeedbackService
{
    private readonly JsonStorageService<Feedback> _storage;

    public FeedbackService()
    {
        _storage = new JsonStorageService<Feedback>("feedbacks.json");
    }

    public async Task<IEnumerable<Feedback>> GetFeedbacksForEventAsync(Guid eventId)
    {
        var feedbacks = await _storage.ReadAllAsync();
        return feedbacks
            .Where(f => f.EventId == eventId)
            .OrderByDescending(f => f.SubmittedAt);
    }

    public async Task<Feedback> SubmitFeedbackAsync(Feedback feedback)
    {
        feedback.Id = Guid.NewGuid();
        feedback.SubmittedAt = DateTime.UtcNow;
        return await _storage.AddAsync(feedback);
    }

    public async Task<bool> DeleteFeedbackAsync(Guid id)
    {
        return await _storage.RemoveAsync(f => f.Id == id);
    }

    public async Task<double> GetAverageRatingAsync(Guid eventId)
    {
        var feedbacks = await _storage.ReadAllAsync();
        var eventFeedbacks = feedbacks
            .Where(f => f.EventId == eventId && f.Rating.HasValue && f.Rating > 0)
            .ToList();

        if (!eventFeedbacks.Any()) return 0;

        return Math.Round(eventFeedbacks.Average(f => f.Rating!.Value), 1);
    }

    public async Task<int> GetFeedbackCountAsync(Guid eventId)
    {
        var feedbacks = await _storage.ReadAllAsync();
        return feedbacks.Count(f => f.EventId == eventId);
    }
}
