using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.PresentationAssetsBuilder.Services;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationAssetsBuilder(this IServiceCollection services)
    {   
        // Register MetadataProcessingService
        services.AddScoped<GenerateMediaSetIndexWorkflow>();
        services.AddScoped<MetadataXmlWorkflow>();
        services.AddScoped<DirectoryInfuseXmlFileGenerator>();
        services.AddScoped<InfuseXmlFileGenerator>();

        return services;
    }
}