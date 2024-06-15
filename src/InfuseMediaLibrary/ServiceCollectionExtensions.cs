
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfuseMediaLibrary(this IServiceCollection services, IConfiguration configuration)
    {   
        // Add configuration sources
        var section = configuration.GetSection(Settings.SectionName);
        services.Configure<Settings>(options => section.Bind(options));

        // Register Engine
        services.AddScoped<Engine>();
        services.AddScoped<TargetDirectoryResolver>();
        services.AddScoped<InfuseMetadataXmlService>();

        return services;
    }
}