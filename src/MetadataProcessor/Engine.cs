using Kurmann.Videoschnitt.MetadataProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Services.Metadata;

namespace Kurmann.Videoschnitt.MetadataProcessor;

/// <summary>
/// Zentrale Steuereinheit für die Metadaten-Verarbeitung.
/// </summary>
public class Engine
{
    private readonly ModuleSettings _moduleSettings;
    private readonly ApplicationSettings _applicationSettings;
    private readonly FFmpegMetadataService _ffmpegMetadataService;
    private readonly ILogger<Engine> _logger;
    private readonly MediaSetService _mediaSetService;
    private readonly MediaPurposeOrganizer _mediaPurposeOrganizer;
    private readonly InputDirectoryReaderService _inputDirectoryReaderService;

    public Engine(ILogger<Engine> logger,
                  IOptions<ModuleSettings> moduleSettings,
                  IOptions<ApplicationSettings> applicationSettings,
                  FFmpegMetadataService ffmpegMetadataService,
                  MediaSetService mediaSetService,
                  MediaPurposeOrganizer mediaPurposeOrganizer,
                  InputDirectoryReaderService inputDirectoryReaderService)
    {
        _moduleSettings = moduleSettings.Value;
        _applicationSettings = applicationSettings.Value;
        _logger = logger;
        _ffmpegMetadataService = ffmpegMetadataService;
        _mediaSetService = mediaSetService;
        _mediaPurposeOrganizer = mediaPurposeOrganizer;
        _inputDirectoryReaderService = inputDirectoryReaderService;
    }

    public async Task<Result<List<MediaSet>>> Start(IProgress<string> progress)
    {
        progress.Report("Steuereinheit für die Metadaten-Verarbeitung gestartet.");

        // Prüfe ob die Einstellungen korrekt geladen wurden
        if (_applicationSettings.InputDirectory == null)
        {
            return Result.Failure<List<MediaSet>>("Eingabeverzeichnis wurde nicht korrekt aus den Einstellungen geladen.");
        }

        // Informiere über das Eingabeverzeichnis
        progress.Report($"Eingangsverzeichnis: {_applicationSettings.InputDirectory}");

        progress.Report("Versuche die Dateien im Eingangsverzeichnis in Mediensets zu organisisieren.");
        var inputDirectoryContent = await _inputDirectoryReaderService.ReadInputDirectoryAsync(_applicationSettings.InputDirectory);
        if (inputDirectoryContent.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Fehler beim Lesen des Eingangsverzeichnisses: {inputDirectoryContent.Error}");
        }

        var mediaFilesByMediaSets = await _mediaSetService.GroupToMediaSets(inputDirectoryContent.Value);
        if (mediaFilesByMediaSets.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Fehler beim Gruppieren der Medien-Dateien in Mediensets: {mediaFilesByMediaSets.Error}");
        }
        _logger.LogInformation("Mediensets erfolgreich gruppiert.");

        _logger.LogInformation("Organisiere die Medien nach ihrem Verwendungszweck.");
        var mediaSets = _mediaPurposeOrganizer.OrganizeMediaByPurpose(mediaFilesByMediaSets.Value);
        if (mediaSets.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Fehler beim Organisieren der Medien nach ihrem Verwendungszweck: {mediaSets.Error}");
        }
        _logger.LogInformation($"Anzahl Mediensets: {mediaSets.Value.Count}");
        _logger.LogInformation("Medien erfolgreich nach ihrem Verwendungszweck organisiert.");

        _logger.LogInformation("Steuereinheit für die Metadaten-Verarbeitung beendet.");
        return Result.Success(mediaSets.Value);
    }

}