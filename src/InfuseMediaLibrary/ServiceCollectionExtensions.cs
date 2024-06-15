
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfuseMediaLibrary(this IServiceCollection services, IConfiguration configuration)
    {   
        // Add configuration sources
        services.Configure<ModuleSettings>(options => configuration.GetSection(ModuleSettings.SectionName).Bind(options));
        services.Configure<ApplicationSettings>(options => configuration.GetSection(ApplicationSettings.SectionName).Bind(options));

        // Register Engine
        services.AddScoped<Engine>();
        services.AddScoped<TargetDirectoryResolver>();
        services.AddScoped<InfuseMetadataXmlService>();

        return services;
    }
}