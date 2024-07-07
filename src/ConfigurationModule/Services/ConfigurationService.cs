using Kurmann.Videoschnitt.ConfigurationModule.Settings;

namespace Kurmann.Videoschnitt.ConfigurationModule.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly Dictionary<Type, object> _settings;

    public ConfigurationService(ApplicationSettings applicationSettings, InfuseMediaLibrarySettings infuseMediaLibrarySettings, MetadataProcessingSettings metadataProcessingSettings)
    {
        _settings = new Dictionary<Type, object>
        {
            { typeof(ApplicationSettings), applicationSettings },
            { typeof(InfuseMediaLibrarySettings), infuseMediaLibrarySettings },
            { typeof(MetadataProcessingSettings), metadataProcessingSettings }
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