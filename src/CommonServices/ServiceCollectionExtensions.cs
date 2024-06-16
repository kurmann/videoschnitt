using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.CommonServices.FileSystem.Unix;

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