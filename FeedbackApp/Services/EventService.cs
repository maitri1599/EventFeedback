using FeedbackApp.Models;
using FeedbackApp.Services.Interfaces;

namespace FeedbackApp.Services;

public class EventService : IEventService
{
    private readonly JsonStorageService<Event> _storage;

    public EventService()
    {
        _storage = new JsonStorageService<Event>("events.json");
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync()
    {
        var events = await _storage.ReadAllAsync();
        return events.OrderByDescending(e => e.EventDate);
    }

    public async Task<Event?> GetEventByIdAsync(Guid id)
    {
        var events = await _storage.ReadAllAsync();
        return events.FirstOrDefault(e => e.Id == id);
    }

    public async Task<Event?> GetEventByTokenAsync(string token)
    {
        var events = await _storage.ReadAllAsync();
        return events.FirstOrDefault(e => e.ShareToken.Equals(token, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Event> CreateEventAsync(Event eventItem)
    {
        eventItem.Id = Guid.NewGuid();
        eventItem.CreatedAt = DateTime.UtcNow;

        for (int i = 0; i < eventItem.Questions.Count; i++)
        {
            eventItem.Questions[i].Id = Guid.NewGuid();
            eventItem.Questions[i].Order = i;
        }

        return await _storage.AddAsync(eventItem);
    }

    public async Task<Event> UpdateEventAsync(Event eventItem)
    {
        var events = await _storage.ReadAllAsync();
        var index = events.FindIndex(e => e.Id == eventItem.Id);

        if (index >= 0)
        {
            events[index] = eventItem;
            await _storage.SaveAllAsync(events);
        }

        return eventItem;
    }

    public async Task<bool> DeleteEventAsync(Guid id)
    {
        return await _storage.RemoveAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
    {
        var events = await _storage.ReadAllAsync();
        return events
            .Where(e => e.EventDate.Date >= DateTime.Today)
            .OrderBy(e => e.EventDate);
    }

    public async Task<IEnumerable<Event>> GetPastEventsAsync()
    {
        var events = await _storage.ReadAllAsync();
        return events
            .Where(e => e.EventDate.Date < DateTime.Today)
            .OrderByDescending(e => e.EventDate);
    }
}
