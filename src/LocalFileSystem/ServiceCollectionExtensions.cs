
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.LocalFileSystem.Services;

namespace Kurmann.Videoschnitt.LocalFileSystem;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocalFileSystemEngine(this IServiceCollection services)
    {   
        // Register Engine
        services.AddScoped<FileTransferService>();

        return services;
    }
}