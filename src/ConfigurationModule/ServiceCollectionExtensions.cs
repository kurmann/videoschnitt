using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kurmann.Videoschnitt.ConfigurationModule.Services;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;

namespace Kurmann.Videoschnitt.ConfigurationModule
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfigurationModule(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind Settings
            services.Configure<ApplicationSettings>(options => configuration.GetSection(ApplicationSettings.SectionName).Bind(options));
            services.Configure<InfuseMediaLibrarySettings>(options => configuration.GetSection(InfuseMediaLibrarySettings.SectionName).Bind(options));
            services.Configure<MetadataProcessingSettings>(options => configuration.GetSection(MetadataProcessingSettings.SectionName).Bind(options));

            // Register Services
            services.AddSingleton<IConfigurationService, ConfigurationService>();

            return services;
        }
    }
}