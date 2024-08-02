using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.LocalIntegration;

/// <summary>
/// Verantwortlich für die Integration von Videodateien in die Infuse-Mediathek
/// </summary>
internal class VideoIntegrator
{
    private readonly IFileOperations _fileOperations;
    private readonly ILogger<VideoIntegrator> _logger;
    private readonly TargetPathService _targetPathService;
    private readonly InfuseMediaLibrarySettings _infuseMediaLibrarySettings;

    public VideoIntegrator(IFileOperations fileOperations, ILogger<VideoIntegrator> logger, TargetPathService targetPathService, IOptions<InfuseMediaLibrarySettings> infuseMediaLibrarySettings)
    {
        _fileOperations = fileOperations;
        _logger = logger;
        _targetPathService = targetPathService;
        _infuseMediaLibrarySettings = infuseMediaLibrarySettings.Value;
    }

    /// <summary>
    /// Integriert die Dateien eines Mediensets in die Infuse-Mediathek
    /// </summary>
    /// <param name="mediaServerFilesDirectory"></param>
    /// <returns></returns>
    public async Task<Result<Maybe<IntegratedMediaServerVideo>>> IntegrateMediaServerFiles(MediaServerFilesDirectory? mediaServerFilesDirectory)
    {
        if (mediaServerFilesDirectory == null)
        {
            _logger.LogInformation("Kein Medienserver-Verzeichnis vorhanden. Es gibt nichts zu integrieren.");
            return Maybe<IntegratedMediaServerVideo>.None;
        }


        _logger.LogInformation("Integriere die Dateien des Medienservers in die Infuse-Mediathek.");
        _logger.LogInformation("Medienserver-Verzeichnis: {mediaServerFilesDirectory}", mediaServerFilesDirectory);

        // Hole alle unterstützten Videodateien aus dem Medienserver-Verzeichnis
        var supportedVideosResult = mediaServerFilesDirectory.GetSupportedVideos();
        if (supportedVideosResult.IsFailure)
        {
            return Result.Failure<Maybe<IntegratedMediaServerVideo>>($"Fehler beim Ermitteln der unterstützten Videodateien im Medienserver-Verzeichnis {mediaServerFilesDirectory}: {supportedVideosResult.Error}");
        }

        if (supportedVideosResult.Value.Count == 0)
        {
            // Informiere, dass keine unterstützten Videodateien gefunden wurden im entsprechenden Verzeichnis und fahre mit dem nächsten Mediensetverzeichnis fort
            _logger.LogInformation("Es wurden keine unterstützten Videodateien im Medienserver-Verzeichnis {mediaServerFilesDirectory} gefunden.", mediaServerFilesDirectory);
            return Maybe<IntegratedMediaServerVideo>.None;

        }

        // Für Medienserver wird nur eine Videodatei erwartet, wenn mehrere Dateien gefunden werden, wird ein Fehler zurückgegeben
        if (supportedVideosResult.Value.Count > 1)
        {
            return Result.Failure<Maybe<IntegratedMediaServerVideo>>($"Es wurden mehrere unterstützte Videodateien im Medienserver-Verzeichnis {mediaServerFilesDirectory} gefunden. Es wird nur eine Videodatei erwartet.");
        }

        // Integriere die Videodatei in die Infuse-Mediathek
        var videoToIntegrate = supportedVideosResult.Value.First();
        var integrationResult = await IntegrateVideoAsync(videoToIntegrate);
        if (integrationResult.IsFailure)
        {
            return Result.Failure<Maybe<IntegratedMediaServerVideo>>($"Fehler beim Integrieren der Videodatei {videoToIntegrate} in die Infuse-Mediathek: {integrationResult.Error}");
        }

        // Hier wurde der Pfad der Videodatei aktualisiert
        return integrationResult;
    }

    /// <summary>
    /// Integriert eine Videodatei in die Infuse-Mediathek und aktualisiert den Dateipfad des supportedVideo-Objekts
    /// </summary>
    /// <param name="supportedVideo"></param>
    /// <returns></returns>
    public async Task<Result<Maybe<IntegratedMediaServerVideo>>> IntegrateVideoAsync(SupportedVideo supportedVideo)
    {
        // Prüfe, ob die Videodatei derzeit verwendet wird
        var isUsedResult = await _fileOperations.IsFileInUseAsync(supportedVideo);
        if (isUsedResult.IsFailure)
        {
            _logger.LogWarning("Fehler beim Prüfen, ob die Datei {File} verwendet wird: {Error}", supportedVideo.Name, isUsedResult.Error);
            return Result.Failure<Maybe<IntegratedMediaServerVideo>>($"Fehler beim Prüfen, ob die Datei {supportedVideo.Name} verwendet wird: {isUsedResult.Error}");
        }
        if (isUsedResult.Value)
        {
            _logger.LogInformation("Die Datei {File} wird derzeit verwendet und wird daher nicht in die Infuse-Mediathek integriert.", supportedVideo.Name);
            return Maybe<IntegratedMediaServerVideo>.None;
        }

        // Ermittle das Zielverzeichnis für die Integration in die Infuse-Mediathek
        var subDirectoryResult = await _targetPathService.GetSubDirectory(supportedVideo);
        if (subDirectoryResult.IsFailure)
        {
            return Result.Failure<Maybe<IntegratedMediaServerVideo>>($"Das Zielverzeichnis für die Integration in die Infuse-Mediathek konnte nicht ermittelt werden: {subDirectoryResult.Error}");
        }

        // Ermittle das effektive Zielverzeichnis
        var targetDirectoryResult = subDirectoryResult.Value.ToFullPath(_infuseMediaLibrarySettings.InfuseMediaLibraryPathLocal);
        if (targetDirectoryResult.IsFailure)
        {
            return Result.Failure<Maybe<IntegratedMediaServerVideo>>($"Das Zielverzeichnis für die Integration in die Infuse-Mediathek konnte nicht ermittelt werden: {targetDirectoryResult.Error}");
        }
        var targetDirectory = targetDirectoryResult.Value;

        if (!targetDirectory.Exists)
        {
            _logger.LogInformation("Das Zielverzeichnis für die Integration in die Infuse-Mediathek existiert nicht. Erstelle Verzeichnis: {targetDirectory.FullName}", targetDirectory.FullName);
            var createDirectoryResult = await _fileOperations.CreateDirectoryAsync(targetDirectory.FullName);
            if (createDirectoryResult.IsFailure)
            {
                return Result.Failure<Maybe<IntegratedMediaServerVideo>>($"Das Zielverzeichnis für die Integration in die Infuse-Mediathek konnte nicht erstellt werden: {targetDirectory.FullName}. Fehler: {createDirectoryResult.Error}");
            }
        }

        var targetFileNameResult = _targetPathService.GetTargetFileName(supportedVideo);
        if (targetFileNameResult.IsFailure)
        {
            return Result.Failure<Maybe<IntegratedMediaServerVideo>>($"Der Ziel-Dateiname für die Integration in die Infuse-Mediathek konnte nicht ermittelt werden: {targetFileNameResult.Error}");
        }

        // Erstelle den Ziel-Dateinamen als Komposition aus dem Zielverzeichnis und dem Dateinamen
        var targetFilePath = Path.Combine(targetDirectory.FullName, targetFileNameResult.Value);

        // Erstelle das Unterzeichnis für die Integration in die Infuse-Mediathek
        if (!Directory.Exists(targetDirectory.FullName))
        {
            _logger.LogInformation("Das Zielverzeichnis für die Integration in die Infuse-Mediathek existiert nicht. Erstelle Verzeichnis: {targetDirectory.FullName}", targetDirectory.FullName);
            var createDirectoryResult = await _fileOperations.CreateDirectoryAsync(targetDirectory.FullName);
            if (createDirectoryResult.IsFailure)
            {
                return Result.Failure<Maybe<IntegratedMediaServerVideo>>($"Das Zielverzeichnis für die Integration in die Infuse-Mediathek konnte nicht erstellt werden: {targetDirectory.FullName}. Fehler: {createDirectoryResult.Error}");
            }
        }

        // Verschiebe die Videodatei in das lokale Infuse-Mediathek-Verzeichnis und überschreibe die Datei falls sie bereits existiert
        var fileMoveResult = await _fileOperations.CopyFileAsync(supportedVideo, targetFilePath, true, true);
        if (fileMoveResult.IsFailure)
        {
            return Result.Failure<Maybe<IntegratedMediaServerVideo>>($"Fehler beim Verschieben der Videodatei {supportedVideo} in das Infuse-Mediathek-Verzeichnis {targetDirectory.FullName}: {fileMoveResult.Error}");
        }

        _logger.LogInformation("Die Video-Datei {videoFile} wurde in das Infuse-Mediathek-Verzeichnis {targetDirectory} verschoben.", supportedVideo, targetDirectory.FullName);
        supportedVideo.UpdateFilePath(targetFilePath);

        var integratedMediaServerVideo = new IntegratedMediaServerVideo(supportedVideo, targetDirectory, subDirectoryResult.Value);
        return Maybe<IntegratedMediaServerVideo>.From(integratedMediaServerVideo);
    }
}

internal record IntegratedMediaServerVideo(SupportedVideo SupportedVideo, DirectoryInfo TargetDirectory, InfuseMediaSubDirectory SubDirectory)
{
    public override string ToString()
    {
        return SupportedVideo.ToString();
    }

    public static implicit operator SupportedVideo(IntegratedMediaServerVideo integratedMediaServerVideo)
    {
        return integratedMediaServerVideo.SupportedVideo;
    }

    public static implicit operator DirectoryInfo(IntegratedMediaServerVideo integratedMediaServerVideo)
    {
        return integratedMediaServerVideo.TargetDirectory;
    }

    public static implicit operator string(IntegratedMediaServerVideo integratedMediaServerVideo)
    {
        return integratedMediaServerVideo.ToString();
    }
}