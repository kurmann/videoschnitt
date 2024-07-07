using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.ConfigurationModule.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly Dictionary<Type, object> _settings;

    public ConfigurationService(IOptions<ApplicationSettings> applicationSettings,
                                IOptions<InfuseMediaLibrarySettings> infuseMediaLibrarySettings,
                                IOptions<MetadataProcessingSettings> metadataProcessingSettings)
    {
        _settings = new Dictionary<Type, object>
        {
            { typeof(ApplicationSettings), applicationSettings.Value },
            { typeof(InfuseMediaLibrarySettings), infuseMediaLibrarySettings.Value },
            { typeof(MetadataProcessingSettings), metadataProcessingSettings.Value }
        };
    }

    public T GetSettings<T>()
    {
        if (_settings.TryGetValue(typeof(T), out var settings))
        {
            return (T)settings;
        }

        throw new InvalidOperationException($"Settings of type {typeof(T).Name} are not registered.");
    }
}