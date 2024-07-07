using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.ConfigurationModule.Services;

/// <summary>
/// Verantwortlich für die Ausgabe von Konfigurationsinformationen.
/// </summary>
public class ConfigurationInfoService
{
    private readonly ILogger<ConfigurationInfoService> _logger;
    private readonly ApplicationSettings _applicationSettings;
    private readonly InfuseMediaLibrarySettings _infuseMediaLibrarySettings;
    private readonly MetadataProcessingSettings _metadataProcessingSettings;

    public ConfigurationInfoService(
        ILogger<ConfigurationInfoService> logger,
        IOptions<ApplicationSettings> applicationSettings,
        IOptions<InfuseMediaLibrarySettings> infuseMediaLibrarySettings,
        IOptions<MetadataProcessingSettings> metadataProcessingSettings)
    {
        _logger = logger;
        _applicationSettings = applicationSettings.Value;
        _infuseMediaLibrarySettings = infuseMediaLibrarySettings.Value;
        _metadataProcessingSettings = metadataProcessingSettings.Value;
    }

    public void LogConfigurationInfo()
    {
        LogApplicationSettings();
        LogInfuseMediaLibrarySettings();
        LogMetadataProcessingSettings();
    }

    private void LogApplicationSettings()
    {
        _logger.LogInformation("Application settings:");
        if (_applicationSettings.IsDefaultInfuseMediaLibraryPath)
        {
            _logger.LogInformation("Kein Infuse Media Library Pfad konfiguriert. Verwende Standardwert: {path}", _applicationSettings.InfuseMediaLibraryPath);
        }
        else
        {
            _logger.LogInformation("Verwende Infuse Media Library Pfad aus der Konfiguration: {path}", _applicationSettings.InfuseMediaLibraryPath);
        }

        if (_applicationSettings.IsDefaultInputDirectory)
        {
            _logger.LogInformation("Kein Eingangsverzeichnis konfiguriert. Verwende Standardwert: {path}", _applicationSettings.InputDirectory);
        }
        else
        {
            _logger.LogInformation("Verwende Eingangsverzeichnis aus der Konfiguration: {path}", _applicationSettings.InputDirectory);
        }
    }

    private void LogInfuseMediaLibrarySettings()
    {
        _logger.LogInformation("Infuse Media Library settings:");
        var bannerFilePostFix = _infuseMediaLibrarySettings.BannerFilePostfix;
        _logger.LogInformation("Banner-Dateiendung: {postfix}", bannerFilePostFix);

        var tempImageSuffix = _infuseMediaLibrarySettings.SuffixForConvertedTempImage;
        _logger.LogInformation("Suffix für konvertiertes temporäres Bild: {suffix}", tempImageSuffix);
    }

    private void LogMetadataProcessingSettings()
    {
        _logger.LogInformation("Metadata processing settings:");

        _logger.LogInformation("Liste der Varianten-Suffixe, die in den Dateinamen von Videos vorkommen können,die für den Medienserver bestimmt sind:");
        foreach (var suffix in _metadataProcessingSettings.MediaSet.VideoVersionSuffixesForMediaServer)
        {
            _logger.LogInformation("{suffix}", suffix);
        }

        _logger.LogInformation("Liste der Varianten-Suffixe, die in den Dateinamen von Videos vorkommen können,die für das Internet bestimmt sind:");
        foreach (var suffix in _metadataProcessingSettings.MediaSet.VideoVersionSuffixesForInternet)
        {
            _logger.LogInformation("{suffix}", suffix);
        }
    }
}
