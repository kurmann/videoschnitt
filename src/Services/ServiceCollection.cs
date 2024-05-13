using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Kurmann.Videoschnitt.Kraftwerk.Hosted;

namespace Kurmann.Videoschnitt.Kraftwerk;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKraftwerk(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bindet Root-Konfigurationswerte an KraftwerkSettings
        services.Configure<KraftwerkSettings>(configuration.GetSection(KraftwerkSettings.SectionName));
        
        // Dienste hinzufügen
        services.AddHostedService<SampleHostedService>();
        
        return services;
    }
}
