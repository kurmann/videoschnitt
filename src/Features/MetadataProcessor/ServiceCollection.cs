using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Kurmann.Videoschnitt.Features.MetadataProcessor.Services;

namespace Kurmann.Videoschnitt.Features.MetadataProcessor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMetadataProcessor(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Settings>(configuration.GetSection(Settings.SectionName));

        services.AddSingleton<MetadataProcessorService>();

        return services;
    }
}