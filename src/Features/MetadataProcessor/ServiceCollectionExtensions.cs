using Microsoft.Extensions.DependencyInjection;

namespace Kurmann.Videoschnitt.Features.MetadataProcessor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMetadataProcessor(this IServiceCollection services)
    {
        // Register MetadataProcessingService
        services.AddSingleton<MetadataProcessingService>();

        return services;
    }
}