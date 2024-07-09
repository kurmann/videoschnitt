using Kurmann.Videoschnitt.MediaSetOrganizer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.Common;
using Kurmann.Videoschnitt.Common.Services.Metadata;
using Kurmann.Videoschnitt.Common.Services.ImageProcessing;
using Kurmann.Videoschnitt.Common.Services.ImageProcessing.MacOS;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Services.FileSystem.Unix;
using Kurmann.Videoschnitt.ConfigurationModule;

namespace Kurmann.Videoschnitt.MediaSetOrganizer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediaSetOrganizer(this IServiceCollection services, IConfiguration configuration)
    {   
        // Register MetadataProcessingService
        services.AddScoped<Workflow>();
        services.AddScoped<FFmpegMetadataService>();
        services.AddScoped<MediaSetService>();
        services.AddScoped<MediaPurposeOrganizer>();
        services.AddScoped<IColorConversionService, MacOSColorConversionService>();
        services.AddScoped<IFileOperations, FileOperations>();
        services.AddScoped<InputDirectoryReaderService>();
        services.AddScoped<MediaSetDirectoryIntegrator>();
        services.AddScoped<FinalCutDirectoryIntegrator>();
        services.AddScoped<ImageProcessorService>();

        services.AddCommonServicesEngine(configuration);
        services.AddConfigurationModule(configuration);

        return services;
    }
}