using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Services.FileSystem.Unix;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfuseMediaLibrary(this IServiceCollection services)
    {   
        // Register Engine
        services.AddScoped<Workflow>();
        services.AddScoped<MediaIntegratorService>();
        services.AddScoped<IFileOperations, FileOperations>();
        services.AddScoped<PosterAndFanartService>();

        return services;
    }
}