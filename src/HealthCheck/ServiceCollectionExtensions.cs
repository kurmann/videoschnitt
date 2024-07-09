using Kurmann.Videoschnitt.HealthCheck.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kurmann.Videoschnitt.HealthCheck;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHealthCheck(this IServiceCollection services)
    {   
        services.AddScoped<Workflow>();
        services.AddScoped<ToolsVersionService>();

        return services;
    }
}