using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Services.Metadata;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.Common.Services.FileSystem.Unix;

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
    private readonly InfuseMediaLibrarySettings _infuseMediaLibrarySettings;

    public MediaIntegratorService(ILogger<MediaIntegratorService> logger, IFileOperations fileOperations, FFmpegMetadataService ffmpegMetadataService, 
        IOptions<InfuseMediaLibrarySettings> infuseMediaLibrarySettings, IOptions<ApplicationSettings> applicationSettings)
    {
        _logger = logger;
        _fileOperations = fileOperations;
        _ffmpegMetadataService = ffmpegMetadataService;
        _applicationSettings = applicationSettings.Value;
        _infuseMediaLibrarySettings = infuseMediaLibrarySettings.Value;
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

        // Ermittle das Album aus den Metadaten der Video-Datei
        var albumResult = await _ffmpegMetadataService.GetMetadataFieldAsync(videoFile, "album");
        if (albumResult.IsFailure)
        {
            return Result.Failure<Maybe<LocalMediaServerFiles>>($"Das Album konnte nicht aus den Metadaten der Video-Datei {videoFile} ermittelt werden: {albumResult.Error}");
        }
        Maybe<string> album = string.IsNullOrWhiteSpace(albumResult.Value) ? Maybe<string>.None : albumResult.Value;
        if (album.HasNoValue)
        {
            _logger.LogTrace("Album-Tag ist nicht in den Metadaten der Video-Datei {FileInfo.Name} vorhanden.", videoFile.Name);
            _logger.LogTrace("Das Album wird für die Integration in die Infuse-Mediathek nicht verwendet.");
        }
        else
        {
            _logger.LogTrace("Album-Tag aus den Metadaten der Video-Datei {FileInfo.Name} ermittelt: {album.Value}", videoFile.Name, album.Value);
            _logger.LogTrace($"Das Album wird für die Integration in die Infuse-Mediathek als erste Verzeichnisebene verwendet.");
        }

        // Ermittle das Aufnahmedatum aus dem Titel der Video-Datei. Das Aufnahemdatum ist als ISO-String im Titel enthalten mit einem Leerzeichen getrennt.
        var recordingDate = GetRecordingDateFromTitle(title);
        if (recordingDate.HasNoValue)
        {
            _logger.LogTrace("Das Aufnahmedatum konnte nicht aus dem Titel der Video-Datei {FileInfo.Name} ermittelt werden.", videoFile.Name);
            _logger.LogTrace("Das Aufnahmedatum wird für die Integration in die Infuse-Mediathek nicht verwendet.");
        }
        else
        {
            _logger.LogTrace("Aufnahmedatum aus dem Titel der Video-Datei {FileInfo.Name} ermittelt: {recordingDate.Value}", videoFile.Name, recordingDate.Value);
            _logger.LogTrace("Das Aufnahmedatum wird für die Integration in die Infuse-Mediathek als zweite Verzeichnisebene verwendet.");
        }

        _logger.LogInformation("Gefunden in den Metadaten der Video-Datei: Album: {album}", album);
        _logger.LogInformation("Ausgelesen aus dem Dateinamen: Aufnahmedatum: {recordingDate}", recordingDate);

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
        var movedSupportedImagesResult = await IntegratedSupportedImagesToInfuseMediaLibrary(imageFiles.ToList(), targetFilePathResult.Value);
        if (movedSupportedImagesResult.IsFailure)
        {
            _logger.LogWarning("Die Bild-Dateien konnten nicht in das Infuse-Mediathek-Verzeichnis {targetDirectory.FullName} verschoben werden: {movedSupportedImagesResult.Error}", targetDirectory.FullName, movedSupportedImagesResult.Error);
            _logger.LogInformation("Es werden keine Bild-Dateien in das Infuse-Mediathek-Verzeichnis verschoben.");
        }

        return Result.Success();
    }

    /// <summary>
    /// Verschiebt die unterstützten Bild-Dateien in das Infuse-Mediathek-Verzeichnis und konvertiert den Farbraum der Bilder in Adobe RGB.
    /// </summary>
    /// <param name="supportedImages"></param>
    /// <param name="videoFileTargetPath"></param>
    /// <returns></returns>
    private async Task<Result> IntegratedSupportedImagesToInfuseMediaLibrary(List<FileInfo> supportedImages, FileInfo videoFileTargetPath)
    {
        // Wenn kein Bild vorhanden sind, wird mit einer Info geloggt und die Methode beendet.
        if (supportedImages.Count == 0)
        {
            _logger.LogInformation($"Keine Bild-Dateien für das Medienset vorhanden.");
            _logger.LogInformation("Es wird kein Bild in das Infuse-Mediathek-Verzeichnis verschoben.");
            return Result.Success();
        }

        // Ermittle das Zielverzeichnis für die Bild-Datei. Dieses ist das gleiche wie das Zielverzeichnis der Video-Datei.
        var videoTargetDirectory = videoFileTargetPath.Directory;
        if (videoTargetDirectory == null)
        {
            return Result.Failure($"Das Verzeichnis der Video-Datei {videoFileTargetPath.FullName} konnte nicht ermittelt werden. Das Verzeichnis wird benötigt, um die Bild-Dateien in das Infuse-Mediathek-Verzeichnis zu verschieben.");
        }

        // Wenn nur ein Bild vorhanden ist, wird dieses als Poster verwendet. Der Name des Bildes entspricht dem Namen der Video-Datei.
        if (supportedImages.Count == 1)
        {
            var supportedImage = supportedImages.First();

            var targetFilePath = Path.Combine(videoTargetDirectory.FullName, videoFileTargetPath.Name.Replace(videoFileTargetPath.Extension, supportedImage.Extension));
            var moveFileResult = await _fileOperations.CopyFileAsync(supportedImage.FullName, targetFilePath);
            if (moveFileResult.IsFailure)
            {
                return Result.Failure($"Die Bild-Datei {supportedImage.FullName} konnte nicht in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben werden. Fehler: {moveFileResult.Error}");
            }
            _logger.LogInformation("Bild-Datei {supportedImage.FileInfo.FullName} erfolgreich in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben.", supportedImage.FullName, videoTargetDirectory.FullName);
            return Result.Success();
        }

        // Wenn mehr als ein Bild vorhanden ist, dann werden die ersten zwei Bilder als Poster und Fanart verwendet und mit Hilfe des PosterAndFanartService die passenden Bilder ermittelt.
        var detectPosterAndFanartImagesResult = PosterAndFanartService.DetectPosterAndFanartImages(supportedImages.ElementAt(0), supportedImages.ElementAt(1));
        if (detectPosterAndFanartImagesResult.IsFailure)
        {
            return Result.Failure($"Das Poster und Fanart konnte nicht ermittelt werden: {detectPosterAndFanartImagesResult.Error}");
        }
        _logger.LogInformation($"Das Poster und Fanart wurde erfolgreich ermittelt.");
        _logger.LogInformation("Poster: {Name}", detectPosterAndFanartImagesResult.Value.PosterImage.Name);
        _logger.LogInformation("Fanart: {Name}", detectPosterAndFanartImagesResult.Value.FanartImage.Name);

        // Das Posterbild hat den gleichen Dateinamen die Videodatei.
        var posterImage = detectPosterAndFanartImagesResult.Value.PosterImage;
        var targetPosterFilePath = Path.Combine(videoTargetDirectory.FullName, videoFileTargetPath.Name.Replace(videoFileTargetPath.Extension, posterImage.Extension));

        // Prüfe ob bereits eine Datei am Zielort existiert mit gleichem Namen und gleichem Änderungsdatum
        if (FileOperations.ExistAtTarget(posterImage.FullName, targetPosterFilePath))
        {
            _logger.LogTrace("Die Poster-Datei {posterImage.FileInfo.FullName} existiert bereits im Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName}. Die Datei wird nicht verschoben.", posterImage.FullName, videoTargetDirectory.FullName);
        }
        else 
        {
            // Kopiere das Posterbild in das Infuse-Mediathek-Verzeichnis
            var movePosterFileResult = await _fileOperations.CopyFileAsync(posterImage.FullName, targetPosterFilePath);
            if (movePosterFileResult.IsFailure)
            {
                return Result.Failure($"Das Posterbild {posterImage.FullName} konnte nicht in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben werden. Fehler: {movePosterFileResult.Error}");
            }
            _logger.LogInformation("Posterbild {FileName} erfolgreich in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben.", posterImage.Name, videoTargetDirectory.FullName);
        }

        // Das Fanartbild hat den gleichen Dateinamen wie die Videodatei und zusätzlich dem Postfix definiert aus den Einstellungen.
        var bannerFilePostfix = _infuseMediaLibrarySettings.BannerFilePostfix;
        if (string.IsNullOrWhiteSpace(bannerFilePostfix))
        {
            return Result.Failure("Das Suffix des Dateinamens, das für die Banner-Datei verwendet wird für die Infuse-Mediathek als Titelbild, ist nicht definiert.");
        }
        var fanartImage = detectPosterAndFanartImagesResult.Value.FanartImage;
        var targetFanartFilePath = Path.Combine(videoTargetDirectory.FullName, videoFileTargetPath.Name.Replace(videoFileTargetPath.Extension, $"{bannerFilePostfix}{fanartImage.Extension}"));

        // Prüfe ob bereits eine Datei am Zielort existiert mit gleichem Namen und gleichem Änderungsdatum
        if (FileOperations.ExistAtTarget(fanartImage.FullName, targetFanartFilePath))
        {
            _logger.LogTrace("Die Fanart-Datei {fanartImage.FileInfo.FullName} existiert bereits im Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName}. Die Datei wird nicht verschoben.", fanartImage.FullName, videoTargetDirectory.FullName);
        }
        else
        {
            // Kopiere das Fanartbild in das Infuse-Mediathek-Verzeichnis
            var moveFanartFileResult = await _fileOperations.CopyFileAsync(fanartImage.FullName, targetFanartFilePath);
            if (moveFanartFileResult.IsFailure)
            {
                return Result.Failure($"Das Fanartbild {fanartImage.FullName} konnte nicht in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben werden. Fehler: {moveFanartFileResult.Error}");
            }
            _logger.LogInformation("Fanartbild {fanartImage.FileInfo.FullName} erfolgreich in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben.", fanartImage.FullName, videoTargetDirectory.FullName);
        }

        return Result.Success();
    }

    /// <summary>
    /// Gibt das Aufnahmedatum aus dem Titel der Video-Datei zurück.
    /// Das Aufnahemdatum ist zu Beginn des Titels als ISO-String enthalten mit einem Leerzeichen getrennt.
    /// </summary>
    /// <param name="videoFile"></param>
    /// <returns></returns>
    private static Maybe<DateOnly> GetRecordingDateFromTitle(string? titleFromMetadata)
    {
        if (string.IsNullOrWhiteSpace(titleFromMetadata))
            return Maybe<DateOnly>.None;

        var titleParts = titleFromMetadata.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (titleParts.Length == 0)
            return Maybe<DateOnly>.None;

        var recordingDate = titleParts[0];
        if (!DateOnly.TryParse(recordingDate, out var recordingDateValue))
            return Maybe<DateOnly>.None;

        return recordingDateValue;
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
