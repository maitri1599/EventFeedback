using FeedbackApp.Models;

namespace FeedbackApp.Services.Interfaces;

public interface IFeedbackService
{
    Task<IEnumerable<Feedback>> GetFeedbacksForEventAsync(Guid eventId);
    Task<Feedback> SubmitFeedbackAsync(Feedback feedback);
    Task<bool> DeleteFeedbackAsync(Guid id);
    Task<double> GetAverageRatingAsync(Guid eventId);
    Task<int> GetFeedbackCountAsync(Guid eventId);
}
