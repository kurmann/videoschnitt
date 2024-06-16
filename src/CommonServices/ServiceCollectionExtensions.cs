using Kurmann.Videoschnitt.LocalFileSystem.FileSystem.Unix;
using Microsoft.Extensions.DependencyInjection;

namespace Kurmann.Videoschnitt.CommonServices;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonServicesEngine(this IServiceCollection services)
    {   
        // Register Engine
        services.AddScoped<FileTransferService>();
        services.AddScoped<ExecuteCommandService>();

        return services;
    }
}