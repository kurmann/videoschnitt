using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Services.Metadata;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.Common.Services.FileSystem.Unix;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.Integration;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

/// <summary>
/// Verantwortlich für die Integration von Mediensets in die lokale Infuse-Mediathek.
/// </summary>
public class MediaIntegratorService
{
    private readonly ILogger<MediaIntegratorService> _logger;
    private readonly IFileOperations _fileOperations;
    private readonly FFmpegMetadataService _ffmpegMetadataService;
    private readonly ApplicationSettings _applicationSettings; 
    private readonly ArtworkImageIntegrator _artworkImageIntegrator;
    private readonly VideoMetadataService _videoMetadataService;

    public MediaIntegratorService(ILogger<MediaIntegratorService> logger, IFileOperations fileOperations, FFmpegMetadataService ffmpegMetadataService, 
        IOptions<ApplicationSettings> applicationSettings, ArtworkImageIntegrator artworkImageIntegrator, VideoMetadataService videoMetadataService)
    {
        _logger = logger;
        _fileOperations = fileOperations;
        _ffmpegMetadataService = ffmpegMetadataService;
        _applicationSettings = applicationSettings.Value;
        _artworkImageIntegrator = artworkImageIntegrator;
        _videoMetadataService = videoMetadataService;
    }

    public async Task<Result> IntegrateMediaSetToLocalInfuseMediaLibrary(string title, FileInfo videoFile, IEnumerable<FileInfo> imageFiles)
    {
        // Prüfe ob das Infuse-Mediathek-Verzeichnis existiert und erstelle es falls es nicht existiert
        if (!Directory.Exists(_applicationSettings.InfuseMediaLibraryPathLocal))
        {
            _logger.LogInformation("Das Infuse-Mediathek-Verzeichnis {infuseMediaLibraryPathLocal} existiert nicht. Erstelle Verzeichnis.", _applicationSettings.InfuseMediaLibraryPathLocal);
            var createDirectoryResult = await _fileOperations.CreateDirectoryAsync(_applicationSettings.InfuseMediaLibraryPathLocal);
            if (createDirectoryResult.IsFailure)
            {
                return Result.Failure<Maybe<LocalMediaServerFiles>>($"Das Infuse-Mediathek-Verzeichnis {createDirectoryResult.Error} konnte nicht erstellt werden.");
            }
            _logger.LogInformation("Infuse-Mediathek-Verzeichnis {infuseMediaLibraryPathLocal} erfolgreich erstellt.", _applicationSettings.InfuseMediaLibraryPathLocal);
        }

        var album = await _videoMetadataService.GetAlbumAsync(videoFile);
        if (album.IsFailure)
        {
            return Result.Failure<Maybe<LocalMediaServerFiles>>($"Das Album konnte nicht aus den Metadaten der Video-Datei {videoFile} ermittelt werden: {album.Error}");
        }

        var recordingDate = _videoMetadataService.GetRecordingDate(title);
        if (recordingDate.IsFailure)
        {
            return Result.Failure<Maybe<LocalMediaServerFiles>>($"Das Aufnahmedatum konnte nicht aus dem Titel der Video-Datei {videoFile} ermittelt werden: {recordingDate.Error}");
        }

        _logger.LogInformation("Gefunden in den Metadaten der Video-Datei: Album: {album}", album.Value);
        _logger.LogInformation("Ausgelesen aus dem Dateinamen: Aufnahmedatum: {recordingDate}", recordingDate.Value.ToLongDateString());

        var targetFilePathResult = GetTargetFilePath(videoFile, album.Value, title, recordingDate.Value);
        if (targetFilePathResult.IsFailure)
        {
            return Result.Failure<Maybe<LocalMediaServerFiles>>($"Das Zielverzeichnis für die Integration in die Infuse-Mediathek konnte nicht ermittelt werden: {targetFilePathResult.Error}");
        }
        _logger.LogInformation("Zielverzeichnis für die Integration in die Infuse-Mediathek ermittelt: {targetFilePathResult.Value.FullName}", targetFilePathResult.Value.FullName);

        // Verschiebe die Video-Datei in das Infuse-Mediathek-Verzeichnis und erstelle das Verzeichnis falls es nicht existiert
        var targetDirectory = targetFilePathResult.Value.Directory;
        if (targetDirectory == null)
        {
            return Result.Failure<Maybe<LocalMediaServerFiles>>($"Das Zielverzeichnis für die Integration in die Infuse-Mediathek konnte nicht ermittelt werden: {targetFilePathResult.Value.FullName}");
        }
        if (!targetDirectory.Exists)
        {
            _logger.LogInformation("Das Zielverzeichnis für die Integration in die Infuse-Mediathek existiert nicht. Erstelle Verzeichnis: {targetDirectory.FullName}", targetDirectory.FullName);
            var createDirectoryResult = await _fileOperations.CreateDirectoryAsync(targetDirectory.FullName);
            if (createDirectoryResult.IsFailure)
            {
                return Result.Failure<Maybe<LocalMediaServerFiles>>($"Das Zielverzeichnis für die Integration in die Infuse-Mediathek konnte nicht erstellt werden: {targetDirectory.FullName}. Fehler: {createDirectoryResult.Error}");
            }
        }

        // Prüfe ob bereits eine Datei am Zielort existiert mit gleichem Namen und gleichem Änderungsdatum
        if (FileOperations.ExistAtTarget(videoFile.FullName, targetFilePathResult.Value.FullName))
        {
            _logger.LogTrace("Die Video existiert bereits im Infuse-Mediathek-Verzeichnis {targetDirectory.FullName}. Die Datei wird nicht verschoben.", targetDirectory.FullName);
        }
        else
        {
            // Kopiere die Video-Datei in das Infuse-Mediathek-Verzeichnis
            var moveFileResult = await _fileOperations.CopyFileAsync(videoFile.FullName, targetFilePathResult.Value.FullName, true);
            if (moveFileResult.IsFailure)
            {
                return Result.Failure<Maybe<LocalMediaServerFiles>>($"Die Video-Datei {videoFile.Name} konnte nicht in das Infuse-Mediathek-Verzeichnis {targetDirectory.FullName} verschoben werden. Fehler: {moveFileResult.Error}");
            }
            _logger.LogInformation("Video-Datei {Filename} erfolgreich in das Infuse-Mediathek-Verzeichnis {targetDirectory} verschoben.", videoFile.Name, targetDirectory.FullName);
        }

        // Integriere die Bild-Dateien in das Infuse-Mediathek-Verzeichnis. Diese haben den gleichen Namen und das gleiche Zielverzeichnis wie die Video-Datei.
        var movedSupportedImagesResult = await _artworkImageIntegrator.IntegrateImages(imageFiles.ToList(), targetFilePathResult.Value);
        if (movedSupportedImagesResult.IsFailure)
        {
            _logger.LogWarning("Die Bild-Dateien konnten nicht in das Infuse-Mediathek-Verzeichnis {targetDirectory.FullName} verschoben werden: {movedSupportedImagesResult.Error}", targetDirectory.FullName, movedSupportedImagesResult.Error);
            _logger.LogInformation("Es werden keine Bild-Dateien in das Infuse-Mediathek-Verzeichnis verschoben.");
        }

        return Result.Success();
    }

    /// <summary>
    /// Gibt das Zielverzeichnis für das Medienset zurück nach folgendem Schema:
    /// <Infuse-Mediathek-Verzeichnis>/<Album>/<Aufnahmedatum.JJJJ>/<Aufnahmedatum.JJJJ-MM-DD>/<Titel ohne ISO-Datum>.<Dateiendung>
    /// </summary>
    /// <param name="album"></param>
    /// <param name="recordingDate"></param>
    /// <returns></returns>
    private Result<FileInfo> GetTargetFilePath(FileInfo supportedVideo, string? album, string? title, DateOnly recordingDate)
    {
        if (string.IsNullOrWhiteSpace(album))
            return Result.Failure<FileInfo>("Das Album ist leer.");
        
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<FileInfo>("Der Titel ist leer.");

        if (supportedVideo == null)
            return Result.Failure<FileInfo>("Die Quelldatei des SupportedVideo-Objekts ist null.");

        var targetDirectory = Path.Combine(_applicationSettings.InfuseMediaLibraryPathLocal, album, recordingDate.Year.ToString(), title);

        // Der Ziel-Dateiname ist ohne vorangestelltes ISO-Datum. Dieses muss also aus dem Titel entfernt werden.
        var titleWithoutLeadingRecordingDate = title.Replace($"{recordingDate:yyyy-MM-dd} ", string.Empty);

        var targetFileName = $"{titleWithoutLeadingRecordingDate}{supportedVideo.Extension}";
        var targetFilePath = Path.Combine(targetDirectory, targetFileName);
    
        return new FileInfo(targetFilePath);
    }
}
