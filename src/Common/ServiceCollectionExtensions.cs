using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.Common.FileSystem.Unix;
using Kurmann.Videoschnitt.Common.FileSystem;

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