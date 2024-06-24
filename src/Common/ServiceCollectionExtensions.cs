using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Services.FileSystem.Unix;

namespace Kurmann.Videoschnitt.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonServicesEngine(this IServiceCollection services, IConfiguration configuration)
    { 
        // Add configuration sources
        services.Configure<ApplicationSettings>(options => configuration.GetSection(ApplicationSettings.SectionName).Bind(options));

        // Register Engine
        services.AddScoped<ExecuteCommandService>();
        services.AddScoped<IFileOperations, FileOperations>();
        services.AddScoped<IFileSearchService, FileSearchService>();

        return services;
    }
}