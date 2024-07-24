using Kurmann.Videoschnitt.MediaSetOrganizer.Services;
using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Kurmann.Videoschnitt.MediaSetOrganizer.Services.Imaging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.MediaSetOrganizer.Services.Metadata;

namespace Kurmann.Videoschnitt.MediaSetOrganizer;

/// <summary>
/// Zentrale Steuereinheit für die Metadaten-Verarbeitung.
/// </summary>
public class Workflow
{
    private readonly ILogger<Workflow> _logger;
    private readonly MediaSetService _mediaSetService;
    private readonly MediaPurposeOrganizer _mediaPurposeOrganizer;
    private readonly InputDirectoryReaderService _inputDirectoryReaderService;
    private readonly MediaSetDirectoryIntegrator _mediaSetDirectoryIntegrator;
    private readonly FinalCutDirectoryIntegrator _finalCutDirectoryIntegrator;
    private readonly ImageProcessorService _imageProcessorService;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;
    private readonly DirectoryInfuseXmlFileGenerator _directoryInfuseXmlFileGenerator;

    public Workflow(ILogger<Workflow> logger, MediaSetService mediaSetService,
        MediaPurposeOrganizer mediaPurposeOrganizer, InputDirectoryReaderService inputDirectoryReaderService,
        MediaSetDirectoryIntegrator mediaSetDirectoryIntegrator, FinalCutDirectoryIntegrator finalCutDirectoryIntegrator,
        ImageProcessorService imageProcessorService, IOptions<MediaSetOrganizerSettings> mediaSetOrganizerSettings, DirectoryInfuseXmlFileGenerator directoryInfuseXmlFileGenerator)
    {
        _logger = logger;
        _mediaSetService = mediaSetService;
        _mediaPurposeOrganizer = mediaPurposeOrganizer;
        _inputDirectoryReaderService = inputDirectoryReaderService;
        _mediaSetDirectoryIntegrator = mediaSetDirectoryIntegrator;
        _finalCutDirectoryIntegrator = finalCutDirectoryIntegrator;
        _imageProcessorService = imageProcessorService;
        _mediaSetOrganizerSettings = mediaSetOrganizerSettings.Value;
        _directoryInfuseXmlFileGenerator = directoryInfuseXmlFileGenerator;
    }

    public async Task<Result<List<MediaSet>>> ExecuteAsync()
    {
        _logger.LogInformation("Verschiebe unterstützte Dateien aus dem Final Cut Pro-Export-Verzeichnis in das Eingangsverzeichnis.");
        var integratedFinalCutFilesResult = await _finalCutDirectoryIntegrator.IntegrateFinalCutExportFilesAsync();
        if (integratedFinalCutFilesResult.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Fehler beim Integrieren der Dateien aus dem Final Cut Pro-Export-Verzeichnis: {integratedFinalCutFilesResult.Error}");
        }

        _logger.LogInformation("Versuche die Dateien im Eingangsverzeichnis in Mediensets zu organisisieren.");
        var inputDirectoryContent = await _inputDirectoryReaderService.ReadInputDirectoryAsync(_mediaSetOrganizerSettings.InputDirectory);
        if (inputDirectoryContent.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Fehler beim Lesen des Eingangsverzeichnisses: {inputDirectoryContent.Error}");
        }

        var mediaFilesByMediaSets = await _mediaSetService.GroupToMediaSets(inputDirectoryContent.Value);
        if (mediaFilesByMediaSets.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Fehler beim Gruppieren der Medien-Dateien in Mediensets: {mediaFilesByMediaSets.Error}");
        }
        if (mediaFilesByMediaSets.Value.Count == 0)
        {
            _logger.LogInformation("Keine unterstützten Dateien im Verzeichnis gefunden oder keine Dateien gefunden, die nicht in Verarbeitung sind.");
            return Result.Success(new List<MediaSet>());
        }
        _logger.LogInformation("Mediensets erfolgreich gruppiert.");

        _logger.LogInformation("Organisiere die Medien nach ihrem Verwendungszweck.");
        var mediaSets = _mediaPurposeOrganizer.OrganizeMediaByPurpose(mediaFilesByMediaSets.Value);
        if (mediaSets.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Fehler beim Organisieren der Medien nach ihrem Verwendungszweck: {mediaSets.Error}");
        }
        _logger.LogInformation("Anzahl Mediensets: {Count}", mediaSets.Value.Count);
        _logger.LogInformation("Medien erfolgreich nach ihrem Verwendungszweck organisiert.");

        _logger.LogInformation("Erstelle JPG-Bilder im Adobe RGB-Farbraum für die Mediensets.");
        var convertColorSpaceResult = await _imageProcessorService.ConvertColorSpaceAndFormatAsync(mediaSets.Value);
        if (convertColorSpaceResult.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Fehler beim Konvertieren des Farbraums und Formats der Bilder: {convertColorSpaceResult.Error}");
        }

        _logger.LogInformation("Verschiebe die Medien in die lokalen Medienset-Verzeichnisse.");
        var integrateInLocalMediaSetDirectoryResult = await _mediaSetDirectoryIntegrator.IntegrateInLocalMediaSetDirectory(mediaSets.Value);
        if (integrateInLocalMediaSetDirectoryResult.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Fehler beim Integrieren der Mediensets in die lokalen Medienset-Verzeichnisse: {integrateInLocalMediaSetDirectoryResult.Error}");
        }

        _logger.LogInformation("Medien erfolgreich in die lokalen Medienset-Verzeichnisse verschoben.");

        // Erstelle die Infuse-XML-Dateien
        var generateInfuseXmlFilesResult = await _directoryInfuseXmlFileGenerator.Generate(_mediaSetOrganizerSettings.MediaSetPathLocal);
        if (generateInfuseXmlFilesResult.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Fehler beim Erstellen der Infuse-XML-Dateien: {generateInfuseXmlFilesResult.Error}");
        }

        // Logge die erstellten Infuse-XML-Dateien
        _logger.LogInformation("Folgende Infuse-XML-Dateien wurden erstellt:");
        foreach (var infuseXmlFile in generateInfuseXmlFilesResult.Value.MetadataFiles)
        {
            _logger.LogInformation("{InfuseXmlFile}", infuseXmlFile.Name);
        }

        _logger.LogInformation("Steuereinheit für die Metadaten-Verarbeitung beendet.");
        return Result.Success(mediaSets.Value);
    }

}