using Kurmann.Videoschnitt.MetadataProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;


namespace Kurmann.Videoschnitt.MetadataProcessor;

/// <summary>
/// Zentrale Steuereinheit für die Metadaten-Verarbeitung.
/// </summary>
public class Engine
{
    private readonly ApplicationSettings _applicationSettings;
    private readonly ILogger<Engine> _logger;
    private readonly MediaSetService _mediaSetService;
    private readonly MediaPurposeOrganizer _mediaPurposeOrganizer;
    private readonly InputDirectoryReaderService _inputDirectoryReaderService;

    public Engine(ILogger<Engine> logger,
                  IOptions<ApplicationSettings> applicationSettings,
                  MediaSetService mediaSetService,
                  MediaPurposeOrganizer mediaPurposeOrganizer,
                  InputDirectoryReaderService inputDirectoryReaderService)
    {
        _applicationSettings = applicationSettings.Value;
        _logger = logger;
        _mediaSetService = mediaSetService;
        _mediaPurposeOrganizer = mediaPurposeOrganizer;
        _inputDirectoryReaderService = inputDirectoryReaderService;
    }

    public async Task<Result<List<MediaSet>>> StartAsync()
    {
        _logger.LogInformation("Steuereinheit für die Metadaten-Verarbeitung gestartet.");

        if (_applicationSettings.InputDirectory == null)
        {
            return Result.Failure<List<MediaSet>>("Eingabeverzeichnis wurde nicht korrekt aus den Einstellungen geladen.");
        }

        _logger.LogInformation("Eingangsverzeichnis: {_applicationSettings.InputDirectory}", _applicationSettings.InputDirectory);

        _logger.LogInformation("Versuche die Dateien im Eingangsverzeichnis in Mediensets zu organisisieren.");
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