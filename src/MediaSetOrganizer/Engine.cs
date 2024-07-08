using Kurmann.Videoschnitt.MediaSetOrganizer.Services;
using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.ConfigurationModule.Services;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;

namespace Kurmann.Videoschnitt.MediaSetOrganizer;

/// <summary>
/// Zentrale Steuereinheit für die Metadaten-Verarbeitung.
/// </summary>
public class Engine
{
    private readonly ILogger<Engine> _logger;
    private readonly MediaSetService _mediaSetService;
    private readonly MediaPurposeOrganizer _mediaPurposeOrganizer;
    private readonly InputDirectoryReaderService _inputDirectoryReaderService;
    private readonly ApplicationSettings _applicationSettings;
    private readonly MediaSetDirectoryIntegrator _mediaSetDirectoryIntegrator;

    public Engine(ILogger<Engine> logger, IConfigurationService configurationService, MediaSetService mediaSetService,
        MediaPurposeOrganizer mediaPurposeOrganizer, InputDirectoryReaderService inputDirectoryReaderService, MediaSetDirectoryIntegrator mediaSetDirectoryIntegrator)
    {
        _logger = logger;
        _applicationSettings = configurationService.GetSettings<ApplicationSettings>();
        _mediaSetService = mediaSetService;
        _mediaPurposeOrganizer = mediaPurposeOrganizer;
        _inputDirectoryReaderService = inputDirectoryReaderService;
        _mediaSetDirectoryIntegrator = mediaSetDirectoryIntegrator;
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

        _logger.LogInformation("Verschiebe die Medien in die lokalen Medienset-Verzeichnisse.");
        var mediaSetDirectories = await _mediaSetDirectoryIntegrator.IntegrateInLocalMediaSetDirectory(mediaFilesByMediaSets.Value);
        if (mediaSetDirectories.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Fehler beim Integrieren der Mediensets in die lokalen Medienset-Verzeichnisse: {mediaSetDirectories.Error}");
        }
        _logger.LogInformation("Medien erfolgreich in die lokalen Medienset-Verzeichnisse verschoben.");

        // todo: die Gruppierung nach Einsatzzweck muss erfolgen nachdem die Dateien bereits verschoben wurden
        _logger.LogInformation("Organisiere die Medien nach ihrem Verwendungszweck.");
        var mediaSets = _mediaPurposeOrganizer.OrganizeMediaByPurpose(mediaFilesByMediaSets.Value);
        if (mediaSets.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Fehler beim Organisieren der Medien nach ihrem Verwendungszweck: {mediaSets.Error}");
        }
        _logger.LogInformation("Anzahl Mediensets: {Count}", mediaSets.Value.Count);
        _logger.LogInformation("Medien erfolgreich nach ihrem Verwendungszweck organisiert.");

        _logger.LogInformation("Steuereinheit für die Metadaten-Verarbeitung beendet.");
        return Result.Success(mediaSets.Value);
    }

}