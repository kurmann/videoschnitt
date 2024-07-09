using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.Common;
using Kurmann.Videoschnitt.ConfigurationModule;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationAssetsBuilder(this IServiceCollection services, IConfiguration configuration)
    {   
        // Register MetadataProcessingService
        services.AddScoped<GenerateMediaSetIndexWorkflow>();
        services.AddScoped<MetadataXmlWorkflow>();

        services.AddCommonServicesEngine(configuration);
        services.AddConfigurationModule(configuration);

        return services;
    }
}