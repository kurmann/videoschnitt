using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.Integration;

internal class VideoIntegratorService
{
    private readonly IFileOperations _fileOperations;
    private readonly ILogger<VideoIntegratorService> _logger;
    private readonly TargetPathService _targetPathService;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;

    public VideoIntegratorService(IFileOperations fileOperations,
        ILogger<VideoIntegratorService> logger,
        IOptions<MediaSetOrganizerSettings> mediaSetOrganizerSettings,
        TargetPathService targetPathService)
    {
        _fileOperations = fileOperations;
        _logger = logger;
        _mediaSetOrganizerSettings = mediaSetOrganizerSettings.Value;
        _targetPathService = targetPathService;
    }

    /// <summary>
    /// Integriert die Dateien eines Mediensets in die Infuse-Mediathek
    /// </summary>
    /// <param name="mediaSetDirectory"></param>
    /// <returns></returns>
    public async Task<Result> IntegrateMediaServerFiles(DirectoryInfo mediaSetDirectory)
    {
        // Prüfe ob das Verzeichnis exisitiert
        if (mediaSetDirectory.Exists == false)
        {
            return Result.Failure($"Das Verzeichnis {mediaSetDirectory} existiert nicht.");
        }

        // Suche nach dem Unterverzeichnis, das die Dateien für den Medienserver enthält
        var mediaServerFileDirectory = mediaSetDirectory.GetDirectories()
            .FirstOrDefault(d => d.Name == _mediaSetOrganizerSettings.MediaSet.MediaServerFilesSubDirectoryName);

        // Ein Medienserver-Verzeichnis ist wird erwartet
        if (mediaServerFileDirectory == null)
        {
            return Result.Failure($"Das Unterverzeichnis {_mediaSetOrganizerSettings.MediaSet.MediaServerFilesSubDirectoryName} wurde im Medienset-Verzeichnis {mediaSetDirectory.FullName} nicht gefunden.");
        }

        // Hole alle unterstützten Videodateien aus dem Medienserver-Verzeichnis
        var supportedVideosResult = SupportedVideo.GetSupportedVideosFromDirectory(mediaServerFileDirectory);
        if (supportedVideosResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Ermitteln der unterstützten Videodateien im Medienserver-Verzeichnis {mediaServerFileDirectory.FullName}. Fehler: {supportedVideosResult.Error}");
        }

        if (supportedVideosResult.Value.Count == 0)
        {
            return Result.Failure($"Keine unterstützten Videodateien im Medienserver-Verzeichnis {mediaServerFileDirectory.FullName} gefunden.");
        }

        // Für Medienserver wird nur eine Datei erwartet, wenn mehrere Dateien gefunden werden, wird ein Fehler zurückgegeben
        if (supportedVideosResult.Value.Count > 1)
        {
            return Result.Failure($"Es wurden mehrere unterstützte Videodateien im Medienserver-Verzeichnis {mediaServerFileDirectory.FullName} gefunden. Es wird nur eine Datei erwartet.");
        }

        // Integriere die Videodatei in die Infuse-Mediathek
        var integrateVideoResult = await IntegrateVideoAsync(supportedVideosResult.Value.First());
        if (integrateVideoResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Integrieren der Videodatei {supportedVideosResult.Value.First()} in die Infuse-Mediathek. Fehler: {integrateVideoResult.Error}");
        }

        return Result.Success();
    }

    public Task<Result> IntegrateVideoAsync(FileInfo videoFile)
    {
        // Prüfe ob die Datei existiert
        if (videoFile.Exists == false)
        {
            return Task.FromResult(Result.Failure($"Die Datei {videoFile} existiert nicht."));
        }

        // Prüfe, ob die Datei eine unterstützte Videodatei ist
        var supportedVideo = SupportedVideo.Create(videoFile);
        if (supportedVideo.IsFailure)
        {
            return Task.FromResult(Result.Failure($"Die Datei {videoFile} ist keine unterstützte Videodatei. Fehler: {supportedVideo.Error}"));
        }

        return IntegrateVideoAsync(supportedVideo.Value);
    }

    public async Task<Result> IntegrateVideoAsync(SupportedVideo supportedVideo)
    {
        // Ermittle das Zielverzeichnis für die Integration in die Infuse-Mediathek
        var targetDirectory = await _targetPathService.GetTargetDirectoryAsync(supportedVideo);
        if (targetDirectory.IsFailure)
        {
            return Result.Failure<Maybe<Result>>($"Das Zielverzeichnis für die Integration in die Infuse-Mediathek konnte nicht ermittelt werden: {targetDirectory.Value.FullName}");
        }
        if (!targetDirectory.Value.Exists)
        {
            _logger.LogInformation("Das Zielverzeichnis für die Integration in die Infuse-Mediathek existiert nicht. Erstelle Verzeichnis: {targetDirectory.FullName}", targetDirectory.Value.FullName);
            var createDirectoryResult = await _fileOperations.CreateDirectoryAsync(targetDirectory.Value.FullName);
            if (createDirectoryResult.IsFailure)
            {
                return Result.Failure<Maybe<Result>>($"Das Zielverzeichnis für die Integration in die Infuse-Mediathek konnte nicht erstellt werden: {targetDirectory.Value.FullName}. Fehler: {createDirectoryResult.Error}");
            }
        }

        var targetFileNameResult = _targetPathService.GetTargetFileName(supportedVideo);
        if (targetFileNameResult.IsFailure)
        {
            return Result.Failure<Maybe<Result>>($"Der Ziel-Dateiname für die Integration in die Infuse-Mediathek konnte nicht ermittelt werden: {targetFileNameResult.Error}");
        }

        // Erstelle den Ziel-Dateinamen als Komposition aus dem Zielverzeichnis und dem Dateinamen
        var targetFilePath = Path.Combine(targetDirectory.Value.FullName, targetFileNameResult.Value);

        // Erstelle das Unterzeichnis für die Integration in die Infuse-Mediathek
        if (!Directory.Exists(targetDirectory.Value.FullName))
        {
            _logger.LogInformation("Das Zielverzeichnis für die Integration in die Infuse-Mediathek existiert nicht. Erstelle Verzeichnis: {targetDirectory.FullName}", targetDirectory.Value.FullName);
            var createDirectoryResult = await _fileOperations.CreateDirectoryAsync(targetDirectory.Value.FullName);
            if (createDirectoryResult.IsFailure)
            {
                return Result.Failure<Maybe<Result>>($"Das Zielverzeichnis für die Integration in die Infuse-Mediathek konnte nicht erstellt werden: {targetDirectory.Value.FullName}. Fehler: {createDirectoryResult.Error}");
            }
        }

        // Verschiebe die Videodatei in das lokale Infuse-Mediathek-Verzeichnis und überschreibe die Datei falls sie bereits existiert
        var fileMoveResult = await _fileOperations.MoveFileAsync(supportedVideo, targetFilePath, true, true);
        if (fileMoveResult.IsFailure)
        {
            return Result.Failure<Maybe<Result>>($"Die Video-Datei {supportedVideo} konnte nicht in das Infuse-Mediathek-Verzeichnis {targetDirectory.Value.FullName} verschoben werden. Fehler: {fileMoveResult.Error}");
        }

        _logger.LogInformation("Die Video-Datei {videoFile} wurde in das Infuse-Mediathek-Verzeichnis {targetDirectory} verschoben.", supportedVideo, targetDirectory.Value.FullName);

        return Result.Success();
    }
}
