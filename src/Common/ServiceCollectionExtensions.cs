using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Services.FileSystem.Unix;

namespace Kurmann.Videoschnitt.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonServicesEngine(this IServiceCollection services)
    {   
        // Register Engine
        services.AddScoped<ExecuteCommandService>();
        services.AddScoped<IFileOperations, FileOperations>();

        return services;
    }
}