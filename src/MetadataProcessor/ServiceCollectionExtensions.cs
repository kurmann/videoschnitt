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
        services.AddSingleton<MetadataProcessingService>();
        services.AddSingleton<MetadataProcessorEngine>();
        services.AddSingleton<MediaFileListenerService>();
        services.AddSingleton<FFmpegMetadataService>();
        services.AddSingleton<CommandExecutorService>();
        services.AddSingleton<MediaTypeDetectorService>();
        services.AddSingleton<MediaSetVariantService>();
        services.AddSingleton<InfuseXmlService>();
        services.AddSingleton<TargetDirectoryResolver>();

        return services;
    }
}