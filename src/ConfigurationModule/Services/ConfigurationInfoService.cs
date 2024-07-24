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
    private readonly InfuseMediaLibrarySettings _infuseMediaLibrarySettings;
    private readonly MediaSetOrganizerSettings _metadataProcessingSettings;

    public ConfigurationInfoService(
        ILogger<ConfigurationInfoService> logger,
        IOptions<InfuseMediaLibrarySettings> infuseMediaLibrarySettings,
        IOptions<MediaSetOrganizerSettings> metadataProcessingSettings)
    {
        _logger = logger;
        _infuseMediaLibrarySettings = infuseMediaLibrarySettings.Value;
        _metadataProcessingSettings = metadataProcessingSettings.Value;
    }

    public void LogConfigurationInfo()
    {
        LogInfuseMediaLibrarySettings();
        LogMediaSetOrganizerSettings();
    }

    private void LogInfuseMediaLibrarySettings()
    {
        _logger.LogTrace("Infuse Media Library settings:");
        var bannerFilePostFix = _infuseMediaLibrarySettings.BannerFilePostfix;
        _logger.LogTrace("Banner-Dateiendung: {postfix}", bannerFilePostFix);

        var tempImageSuffix = _infuseMediaLibrarySettings.SuffixForConvertedTempImage;
        _logger.LogTrace("Suffix für konvertiertes temporäres Bild: {suffix}", tempImageSuffix);
    }

    private void LogMediaSetOrganizerSettings()
    {
        _logger.LogTrace("Metadata processing settings:");

        _logger.LogTrace("Liste der Varianten-Suffixe, die in den Dateinamen von Videos vorkommen können,die für den Medienserver bestimmt sind:");
        var commaSeparatedVideoVersionSuffixesForMediaServer = string.Join(", ", _metadataProcessingSettings.MediaSet.VideoVersionSuffixesForMediaServer);
        _logger.LogTrace("{suffixes}", commaSeparatedVideoVersionSuffixesForMediaServer);

        _logger.LogTrace("Liste der Varianten-Suffixe, die in den Dateinamen von Videos vorkommen können,die für das Internet bestimmt sind:");
        var commaSeparatedVideoVersionSuffixesForInternet = string.Join(", ", _metadataProcessingSettings.MediaSet.VideoVersionSuffixesForInternet);
        _logger.LogTrace("{suffixes}", commaSeparatedVideoVersionSuffixesForInternet);
    }
}
