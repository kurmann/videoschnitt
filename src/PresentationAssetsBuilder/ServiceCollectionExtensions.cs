using Microsoft.Extensions.DependencyInjection;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationAssetsBuilder(this IServiceCollection services)
    {   
        // Register MetadataProcessingService
        services.AddScoped<GenerateMediaSetIndexWorkflow>();

        return services;
    }
}