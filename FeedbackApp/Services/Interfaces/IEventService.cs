using FeedbackApp.Models;

namespace FeedbackApp.Services.Interfaces;

public interface IEventService
{
    Task<IEnumerable<Event>> GetAllEventsAsync();
    Task<Event?> GetEventByIdAsync(Guid id);
    Task<Event?> GetEventByTokenAsync(string token);
    Task<Event> CreateEventAsync(Event eventItem);
    Task<Event> UpdateEventAsync(Event eventItem);
    Task<bool> DeleteEventAsync(Guid id);
    Task<IEnumerable<Event>> GetUpcomingEventsAsync();
    Task<IEnumerable<Event>> GetPastEventsAsync();
}
