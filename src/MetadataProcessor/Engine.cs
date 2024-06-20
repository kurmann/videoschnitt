using Kurmann.Videoschnitt.MetadataProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CSharpFunctionalExtensions;

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
    private readonly MediaSetSubDirectoryOrganizer _mediaSetSubDirectoryOrganizer;

    public Engine(ILogger<Engine> logger,
                  IOptions<ModuleSettings> moduleSettings,
                  IOptions<ApplicationSettings> applicationSettings,
                  FFmpegMetadataService ffmpegMetadataService,
                  MediaSetSubDirectoryOrganizer mediaSetSubDirectoryOrganizer,
                  MediaSetService mediaSetService)
    {
        _moduleSettings = moduleSettings.Value;
        _applicationSettings = applicationSettings.Value;
        _logger = logger;
        _ffmpegMetadataService = ffmpegMetadataService;
        _mediaSetService = mediaSetService;
        _mediaSetSubDirectoryOrganizer = mediaSetSubDirectoryOrganizer;
    }

    public async Task<Result<List<MediaSetDirectory>>> Start(IProgress<string> progress)
    {
        progress.Report("Steuereinheit für die Metadaten-Verarbeitung gestartet.");

        // Prüfe ob die Einstellungen korrekt geladen wurden
        if (_applicationSettings.InputDirectory == null)
        {
            return Result.Failure<List<MediaSetDirectory>>("Eingabeverzeichnis wurde nicht korrekt aus den Einstellungen geladen.");
        }

        // Informiere über das Eingabeverzeichnis
        progress.Report($"Eingangsverzeichnis: {_applicationSettings.InputDirectory}");

        _logger.LogInformation("Versuche die Dateien im Eingangsverzeichnis in Medienset zu organisiseren.");
        var mediaSets = await _mediaSetService.GroupToMediaSets(_applicationSettings.InputDirectory);
        if (mediaSets.IsFailure)
        {
            return Result.Failure<List<MediaSetDirectory>>($"Fehler beim Gruppieren der Medien-Dateien in Mediensets: {mediaSets.Error}");
        }
        _logger.LogInformation("Mediensets erfolgreich gruppiert.");

        _logger.LogInformation("Verschiebe jedes Medienset in ein Unterverzeichnis mit dem Titel des Mediensets.");
        var mediaSetDirectories = _mediaSetSubDirectoryOrganizer.MoveMediaSetsToDirectories(mediaSets.Value, _applicationSettings.InputDirectory);
        if (mediaSetDirectories.IsFailure)
        {
            return Result.Failure<List<MediaSetDirectory>>($"Fehler beim Verschieben der Mediensets in Unterverzeichnisse: {mediaSetDirectories.Error}");
        }

        return Result.Success(mediaSetDirectories.Value);
    }

}