using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Services.FileSystem.Unix;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.Integration;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfuseMediaLibrary(this IServiceCollection services)
    {   
        // Register Engine
        services.AddScoped<IWorkflow, Workflow>();
        services.AddScoped<IFileOperations, FileOperations>();
        services.AddScoped<PosterAndFanartService>();
        services.AddScoped<ArtworkImageIntegrator>();
        services.AddScoped<VideoIntegrator>();
        services.AddScoped<VideoMetadataService>();
        services.AddScoped<TargetPathService>();
        services.AddScoped<ArtworkDirectoryReader>();
        services.AddScoped<LocalMediaSetDirectoryReader>();
        services.AddScoped<MediaSetIntegrator>();

        return services;
    }
}