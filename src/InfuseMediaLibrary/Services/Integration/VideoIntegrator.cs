using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.Integration;

/// <summary>
/// Verantwortlich für die Integration von Videodateien in die Infuse-Mediathek
/// </summary>
internal class VideoIntegrator
{
    private readonly IFileOperations _fileOperations;
    private readonly ILogger<VideoIntegrator> _logger;
    private readonly TargetPathService _targetPathService;

    public VideoIntegrator(IFileOperations fileOperations, ILogger<VideoIntegrator> logger, TargetPathService targetPathService)
    {
        _fileOperations = fileOperations;
        _logger = logger;
        _targetPathService = targetPathService;
    }

    /// <summary>
    /// Integriert die Dateien eines Mediensets in die Infuse-Mediathek
    /// </summary>
    /// <param name="mediaServerFilesDirectory"></param>
    /// <returns></returns>
    public Task<Result> IntegrateMediaServerFiles(MediaServerFilesDirectory mediaServerFilesDirectory)
    {
        // Hole alle unterstützten Videodateien aus dem Medienserver-Verzeichnis
        var supportedVideosResult = mediaServerFilesDirectory.GetSupportedVideos();
        if (supportedVideosResult.IsFailure)
        {
            return Task.FromResult(Result.Failure($"Fehler beim Ermitteln der unterstützten Videodateien im Medienserver-Verzeichnis {mediaServerFilesDirectory}. Fehler: {supportedVideosResult.Error}"));
        }

        if (supportedVideosResult.Value.Count == 0)
        {
            return Task.FromResult(Result.Failure($"Es wurden keine unterstützten Videodateien im Medienserver-Verzeichnis {mediaServerFilesDirectory} gefunden."));
        }

        // Für Medienserver wird nur eine Videodatei erwartet, wenn mehrere Dateien gefunden werden, wird ein Fehler zurückgegeben
        if (supportedVideosResult.Value.Count > 1)
        {
            return Task.FromResult(Result.Failure($"Es wurden mehrere unterstützte Videodateien im Medienserver-Verzeichnis {mediaServerFilesDirectory} gefunden. Es wird nur eine Videodatei erwartet."));
        }

        // Integriere die Videodatei in die Infuse-Mediathek
        return IntegrateVideoAsync(supportedVideosResult.Value.First());
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
