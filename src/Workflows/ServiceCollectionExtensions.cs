using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.HealthCheck;
using Microsoft.Extensions.Configuration;
using Kurmann.Videoschnitt.MetadataProcessor;
using Kurmann.Videoschnitt.InfuseMediaLibrary;
using Kurmann.Videoschnitt.ConfigurationModule;

namespace Kurmann.Videoschnitt.Workflows;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkflows(this IServiceCollection services, IConfiguration configuration)
    {
        // Add configuration module first
        services.AddConfigurationModule(configuration);

        // Add feature modules
        services.AddMetadataProcessor(configuration);
        services.AddInfuseMediaLibrary(configuration);
        services.AddHealthCheck();

        // Register workflows
        services.AddScoped<HealthCheckWorkflow>();
        services.AddScoped<FinalCutProWorkflow>();

        return services;
    }
}