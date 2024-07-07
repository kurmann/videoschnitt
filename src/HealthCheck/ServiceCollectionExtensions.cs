using Kurmann.Videoschnitt.HealthCheck.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Kurmann.Videoschnitt.HealthCheck;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHealthCheck(this IServiceCollection services, IConfiguration applicationSettings)
    {   
        // Add configuration sources
        services.Configure<ApplicationSettings>(options => applicationSettings.GetSection(ApplicationSettings.SectionName).Bind(options));

        // Register HealthCheckService
        services.AddScoped<Engine>();
        services.AddScoped<ToolsVersionService>();

        return services;
    }
}