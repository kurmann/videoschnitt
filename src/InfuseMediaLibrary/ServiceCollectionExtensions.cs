
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfuseMediaLibrary(this IServiceCollection services, IConfiguration configuration)
    {   
        // Add configuration sources
        var section = configuration.GetSection(Settings.SectionName);
        services.Configure<Settings>(options => section.Bind(options));

        return services;
    }
}