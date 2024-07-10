using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Services.FileSystem.Unix;
using Kurmann.Videoschnitt.ConfigurationModule;
using Kurmann.Videoschnitt.Common.Services.Metadata;

namespace Kurmann.Videoschnitt.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonServicesEngine(this IServiceCollection services, IConfiguration configuration)
    { 
        // Register Services
        services.AddScoped<ExecuteCommandService>();
        services.AddScoped<IFileOperations, FileOperations>();
        services.AddScoped<IFileSearchService, FileSearchService>();
        services.AddScoped<FFmpegMetadataService>();
        services.AddScoped<SipsMetadataService>();

        // Register Feature Modules
        services.AddConfigurationModule(configuration);

        return services;
    }
}