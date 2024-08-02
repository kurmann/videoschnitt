using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Services.FileSystem.Unix;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.LocalIntegration;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.RemoteIntegration;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfuseMediaLibrary(this IServiceCollection services)
    {   
        services.AddScoped<IWorkflow, Workflow>();
        services.AddScoped<IFileOperations, FileOperations>();
        services.AddScoped<ArtworkImageIntegrator>();
        services.AddScoped<VideoIntegrator>();
        services.AddScoped<VideoMetadataService>();
        services.AddScoped<TargetPathService>();
        services.AddScoped<ArtworkDirectoryReader>();
        services.AddScoped<LocalMediaSetDirectoryReader>();
        services.AddScoped<MediaSetIntegrator>();
        services.AddScoped<MetadataFileIntegrator>();
        services.AddScoped<InfuseMediaIntegrator>();
        services.AddScoped<DirectoryCleanupService>();

        return services;
    }
}