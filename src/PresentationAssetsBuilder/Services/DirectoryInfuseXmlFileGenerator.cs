using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Services.Metadata;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Kurmann.Videoschnitt.PresentationAssetsBuilder.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder.Services;

/// <summary>
/// Verantwortlich für das Erstellen von Infuse-XML-Dateien aus allen Videodateien eines Verzeichnisses.
/// </summary>
public class DirectoryInfuseXmlFileGenerator
{
    private readonly ILogger<DirectoryInfuseXmlFileGenerator> _logger;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;
    private readonly InfuseXmlFileGenerator _infuseXmlService;

    public DirectoryInfuseXmlFileGenerator(ILogger<DirectoryInfuseXmlFileGenerator> logger, IOptions<MediaSetOrganizerSettings> applicationSettings, InfuseXmlFileGenerator infuseXmlService)
    {
        _logger = logger;
        _mediaSetOrganizerSettings = applicationSettings.Value;
        _infuseXmlService = infuseXmlService;
    }

    /// <summary>
    /// Erstellt für jede Videodatei in einem Verzeichnis eine Infuse-XML-Datei.
    /// </summary>
    /// <param name="inputDirectory"></param>
    /// <returns></returns>
    public async Task<Result<GeneratedMetadataFiles>> Generate(string inputDirectory)
    {
        // Lese alle Verzeichnisse im Eingabeverzeichnis und nimm an, dass es sich um Mediensets handelt
        var mediaSetDirectoryInfos = new DirectoryInfo(inputDirectory).GetDirectories().ToList();
        _logger.LogInformation("Es wurden {mediaSetDirectoryInfos.Count} Mediensets gefunden", mediaSetDirectoryInfos.Count);

        // Ziel ist es in jedem Mediaset zu jeder Videodatei eine Infuse-XML-Datei zu erstellen
        var generatedMetadataFilesByMediaSetList = new List<GeneratedMetadataFilesByMediaSet>();
        foreach (var mediaSetDirectoryInfo in mediaSetDirectoryInfos)
        {
            // Parse den Medienset-Namen, der hier dem Verzeichnisnamen entspricht
            var mediaSetNameResult = MediaSetName.Create(mediaSetDirectoryInfo.Name);
            if (mediaSetNameResult.IsFailure)
            {
                return Result.Failure<GeneratedMetadataFiles>($"Fehler beim Parsen des Medienset-Namens: {mediaSetNameResult.Error}");
            }

            // Filtere alle Videodateien mit dem richtigen Dateiendungen einschließlich Unterverzeichnisse
            var supportedMediaByMediaSet = mediaSetDirectoryInfo.GetFiles("*", SearchOption.AllDirectories)
                .Where(file => _mediaSetOrganizerSettings.MediaSet.SupportedVideoExtensions.Contains(file.Extension.ToLower()))
                .ToList();
            _logger.LogInformation("Im Medienset {mediaSetTitle} wurden {supportedMediaByMediaSet.Count} unterstützte Medien gefunden", mediaSetNameResult.Value, supportedMediaByMediaSet.Count);

            // Erstelle für jede Videodatei die Metadaten-Dateien
            foreach (var mediaFile in supportedMediaByMediaSet)
            {
                // Erstelle die RAW-Metadaten-Datei
                var createRawMetadataFileResult = await _infuseXmlService.GenerateRawFile(mediaFile.FullName);
                if (createRawMetadataFileResult.IsFailure)
                {
                    _logger.LogWarning("Fehler beim Erstellen der RAW-Metadaten-Datei für {mediaFile.Name}: {createRawMetadataFileResult.Error}", mediaFile.Name, createRawMetadataFileResult.Error);
                    _logger.LogInformation("Überspringe die Videodatei {mediaFile.Name}", mediaFile.Name);
                    continue;
                }

                generatedMetadataFilesByMediaSetList.Add(new GeneratedMetadataFilesByMediaSet(createRawMetadataFileResult.Value.MetadataFile, createRawMetadataFileResult.Value.Metadata));

                // Wenn die betreffende Datei eine QuickTime-Datei ist, dann erstelle im Wurzelverzeichnis des Mediensets eine Infuse-XML-Datei
                // Die QuickTime-Datei hat mehr Metadaten als die MPEG-4-Dateien, deshalb dient sie als Referenz für die Infuse-XML-Datei
                if (mediaFile.Extension.Equals(".mov", StringComparison.CurrentCultureIgnoreCase))
                {
                    var customProductionInfuseMetadata = CustomProductionInfuseMetadata.CreateFromFfprobeMetadata(createRawMetadataFileResult.Value.Metadata, mediaSetNameResult.Value.Date);
                    if (customProductionInfuseMetadata.IsFailure)
                    {
                        return Result.Failure<GeneratedMetadataFiles>($"Fehler beim Erstellen der Infuse-XML-Datei für {mediaFile.Name}: {customProductionInfuseMetadata.Error}");
                    }
                    var customProductionInfuseMetadataXmlDoc = customProductionInfuseMetadata.Value.ToXml();

                    // Schreibe den Inhalt der XML-Datei in das Wurzelverzeichnis des Mediensets mit dem gleichen Dateinamen wie das Medienset
                    customProductionInfuseMetadataXmlDoc.Save(Path.Combine(mediaSetDirectoryInfo.FullName, $"{mediaSetNameResult.Value}.xml"));

                }
            }
        }

        return Result.Success(new GeneratedMetadataFiles(generatedMetadataFilesByMediaSetList));
    }

}

public record GeneratedMetadataFiles(List<GeneratedMetadataFilesByMediaSet> MetadataFiles);

public record GeneratedMetadataFilesByMediaSet(FileInfo FilePath, FFprobeMetadata Metadata);
