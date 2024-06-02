using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Kurmann.Videoschnitt.Features.MetadataProcessor.Services;

namespace Kurmann.Videoschnitt.Features.MetadataProcessor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMetadataProcessor(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind Root-Konfigurationswerte an Settings
        services.Configure<Settings>(configuration.GetSection("MetadataProcessor"));

        // Register Processor as a hosted service
        services.AddHostedService<Processor>();

        return services;
    }
}