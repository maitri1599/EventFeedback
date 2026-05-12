using FeedbackApp.Services;
using FeedbackApp.Services.Interfaces;

namespace FeedbackApp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IEventService, EventService>();
        services.AddSingleton<IFeedbackService, FeedbackService>();

        return services;
    }

    public static IServiceCollection AddSecureSession(this IServiceCollection services, int timeoutMinutes = 30)
    {
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(timeoutMinutes);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

        return services;
    }
}
