using Microsoft.Extensions.DependencyInjection;

namespace Kurmann.Videoschnitt.Workflows;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkflows(this IServiceCollection services)
    {   
        services.AddScoped<HealthCheckWorkflow>();
        services.AddScoped<FinalCutProWorkflow>();

        return services;
    }
}