using Kurmann.Videoschnitt.MetadataProcessor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kurmann.Videoschnitt.MetadataProcessor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMetadataProcessor(this IServiceCollection services, IConfiguration configuration)
    {   
        // Add configuration sources
        services.Configure<MetadataProcessorSettings>(configuration.GetSection("MetadataProcessing"));

        // Register MetadataProcessingService
        services.AddSingleton<MetadataProcessingService>();
        services.AddSingleton<MetadataProcessorEngine>();

        return services;
    }
}