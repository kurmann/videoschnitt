using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.CommonServices.FileSystem.Unix;
using Kurmann.Videoschnitt.CommonServices.FileSystem;

namespace Kurmann.Videoschnitt.CommonServices;

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