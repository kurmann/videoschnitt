using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Kurmann.Videoschnitt.Features.MetadataProcessor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMetadataProcessor(this IServiceCollection services)
    {
        // Register MetadataProcessingService
        services.AddSingleton<MetadataProcessingService>();

        // Wolverine und Handler registrieren
        services.AddWolverine(opts =>
        {
            opts.Handlers.Include<MetadataProcessedEventHandler>();
        });

        return services;
    }
}