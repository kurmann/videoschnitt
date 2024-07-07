using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.HealthCheck;
using Microsoft.Extensions.Configuration;
using Kurmann.Videoschnitt.MetadataProcessor;
using Kurmann.Videoschnitt.InfuseMediaLibrary;

namespace Kurmann.Videoschnitt.Workflows;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkflows(this IServiceCollection services, IConfiguration configuration)
    {
        // Add configuration sources to the DI container
        services.AddMetadataProcessor(configuration);
        services.AddInfuseMediaLibrary(configuration);

        // Register workflows
        services.AddScoped<HealthCheckWorkflow>();
        services.AddScoped<FinalCutProWorkflow>();

        services.AddHealthCheck();

        return services;
    }
}