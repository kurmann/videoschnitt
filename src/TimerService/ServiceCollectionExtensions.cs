using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.TimerService.Services;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.TimerService;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTimerServices(this IServiceCollection services)
    {
        // Register MetadataProcessingService
        services.AddHostedService<TimerTriggerService>();

        return services;
    }
}