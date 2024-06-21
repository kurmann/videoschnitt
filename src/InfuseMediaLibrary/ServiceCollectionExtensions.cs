using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services;
using Kurmann.Videoschnitt.Common;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Services.FileSystem.Unix;

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
        services.AddScoped<InfuseMetadataXmlService>();
        services.AddScoped<MediaIntegratorService>();
        services.AddScoped<IFileOperations, FileOperations>();
        services.AddScoped<PosterAndFanartService>();

        services.AddCommonServicesEngine();

        return services;
    }
}