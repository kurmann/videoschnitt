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
    private readonly MediaSetOrganizerSettings _metadataProcessingSettings;

    public ConfigurationInfoService(
        ILogger<ConfigurationInfoService> logger,
        IOptions<ApplicationSettings> applicationSettings,
        IOptions<InfuseMediaLibrarySettings> infuseMediaLibrarySettings,
        IOptions<MediaSetOrganizerSettings> metadataProcessingSettings)
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
            _logger.LogInformation("Sie können den Infuse Media Library Pfad in der appsettings.json-Datei unter dem Schlüssel: {key} konfigurieren", ApplicationSettings.InfuseMediaLibraryPathConfigKey);
        }
        else
        {
            _logger.LogInformation("Verwende Infuse Media Library Pfad aus der Konfiguration: {path}", _applicationSettings.InfuseMediaLibraryPath);
        }

        if (_applicationSettings.IsDefaultInputDirectory)
        {
            _logger.LogInformation("Kein Eingangsverzeichnis konfiguriert. Verwende Standardwert: {path}", _applicationSettings.InputDirectory);
            _logger.LogInformation("Sie können das Eingangsverzeichnis in der appsettings.json-Datei unter dem Schlüssel: {key} konfigurieren", ApplicationSettings.InputDirectoryConfigKey);
        }
        else
        {
            _logger.LogInformation("Verwende Eingangsverzeichnis aus der Konfiguration: {path}", _applicationSettings.InputDirectory);
        }

        if (_applicationSettings.IsDefaultMediaSetPathLocal)
        {
            _logger.LogInformation("Kein lokales Medienset-Verzeichnis konfiguriert. Verwende Standardwert: {path}", _applicationSettings.MediaSetPathLocal);
            _logger.LogInformation("Sie können das lokale Medienset-Verzeichnis in der appsettings.json-Datei unter dem Schlüssel: {key} konfigurieren", ApplicationSettings.MediaSetPathLocalConfigKey);
        }
        else
        {
            _logger.LogInformation("Verwende lokales Medienset-Verzeichnis aus der Konfiguration: {path}", _applicationSettings.MediaSetPathLocal);
        }

        if (_applicationSettings.MediaSetPathRemote == null)
        {
            _logger.LogInformation("Kein externes Medienset-Verzeichnis konfiguriert.");
            _logger.LogInformation("Ein externes Medienset-Verzeichnis ist optional und wird nur benötigt, wenn Mediensets auf einem externen Archiv oder Medienserver abgelegt werden.");
            _logger.LogInformation("Konfigurieren Sie das externe Medienset-Verzeichnis in der appsettings.json-Datei unter dem Schlüssel: {key}", ApplicationSettings.MediaSetPathRemoteConfigKey);
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