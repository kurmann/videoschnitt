using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.HealthCheck;

namespace Kurmann.Videoschnitt.Workflows;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkflows(this IServiceCollection services)
    {   
        services.AddScoped<HealthCheckWorkflow>();
        services.AddScoped<FinalCutProWorkflow>();

        services.AddHealthCheck();

        return services;
    }
}