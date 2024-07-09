using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder.Services;

/// <summary>
/// Verantwortlich für das Erstellen von Infuse-XML-Dateien aus allen Videodateien eines Verzeichnisses.
/// </summary>
public class InfuseXmlFileGenarator
{
    private readonly ILogger<InfuseXmlFileGenarator> _logger;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;

    public InfuseXmlFileGenarator(ILogger<InfuseXmlFileGenarator> logger, IOptions<MediaSetOrganizerSettings> applicationSettings)
    {
        _logger = logger;
        _mediaSetOrganizerSettings = applicationSettings.Value;
    }

    public Task<Result<List<FileInfo>>> GenerateInfuseXmlFiles(string inputDirectory)
    {
        // Lese alle Verzeichnisse im Eingabeverzeichnis und nimm an, dass es sich um Mediensets handelt
        var mediaSetDirectoryInfos = new DirectoryInfo(inputDirectory).GetDirectories().ToList();
        _logger.LogInformation("Es wurden {mediaSetDirectoryInfos.Count} Mediensets gefunden", mediaSetDirectoryInfos.Count);

        // Ziel ist es in jedem Mediaset zu jeder Videodatei eine Infuse-XML-Datei zu erstellen
        foreach (var mediaSetDirectoryInfo in mediaSetDirectoryInfos)
        {
            // Filtere alle Videodateien mit dem richtigen Dateiendungen einschließlich Unterverzeichnisse
            var supportedMediaByMediaSet = mediaSetDirectoryInfo.GetFiles("*", SearchOption.AllDirectories)
                .Where(file => _mediaSetOrganizerSettings.MediaSet.SupportedVideoExtensions.Contains(file.Extension.ToLower()))
                .ToList();

            // Der Titel des Mediensets ist der Verzeichnisname
            var mediaSetTitle = mediaSetDirectoryInfo.Name;

            _logger.LogInformation("Im Medienset {mediaSetTitle} wurden {supportedMediaByMediaSet.Count} unterstützte Medien gefunden", mediaSetTitle, supportedMediaByMediaSet.Count);
        }

        var infuseXmlFiles = new List<FileInfo>();
        return Task.FromResult(Result.Success(infuseXmlFiles));
    }
}
