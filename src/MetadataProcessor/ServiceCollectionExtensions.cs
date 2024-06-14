using Kurmann.Videoschnitt.MetadataProcessor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kurmann.Videoschnitt.MetadataProcessor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMetadataProcessor(this IServiceCollection services, IConfiguration configuration)
    {   
        // Add configuration sources
        var section = configuration.GetSection(MetadataProcessorSettings.SectionName);
        services.Configure<MetadataProcessorSettings>(options => section.Bind(options));

        // Register MetadataProcessingService
        services.AddScoped<MetadataProcessingService>();
        services.AddScoped<MetadataProcessorEngine>();
        services.AddScoped<MediaFileListenerService>();
        services.AddScoped<FFmpegMetadataService>();
        services.AddScoped<CommandExecutorService>();
        services.AddScoped<MediaTypeDetectorService>();
        services.AddScoped<MediaSetVariantService>();
        services.AddScoped<InfuseXmlService>();
        services.AddScoped<TargetDirectoryResolver>();

        return services;
    }
}