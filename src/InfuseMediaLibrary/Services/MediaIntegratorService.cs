using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.Common.Services.Metadata;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

public class MediaIntegratorService
{
    private readonly ILogger<MediaIntegratorService> _logger;
    private readonly IFileOperations _fileOperations;
    private readonly FFmpegMetadataService _ffmpegMetadataService;
    private readonly PosterAndFanartService _posterAndFanartService;
    private readonly ApplicationSettings _applicationSettings; 
    private readonly InfuseMediaLibrarySettings _infuseMediaLibrarySettings;

    public MediaIntegratorService(ILogger<MediaIntegratorService> logger,
                                  IFileOperations fileOperations,
                                  FFmpegMetadataService ffmpegMetadataService,
                                  PosterAndFanartService posterAndFanartService,
                                  IOptions<InfuseMediaLibrarySettings> infuseMediaLibrarySettings,
                                  IOptions<ApplicationSettings> applicationSettings)
    {
        _logger = logger;
        _fileOperations = fileOperations;
        _ffmpegMetadataService = ffmpegMetadataService;
        _posterAndFanartService = posterAndFanartService;
        _applicationSettings = applicationSettings.Value;
        _infuseMediaLibrarySettings = infuseMediaLibrarySettings.Value;
    }

    public async Task<Result> IntegrateMediaSetToLocalInfuseMediaLibrary(FileInfo videoFile, IEnumerable<FileInfo> imageFiles)
    {
        return Result.Success();
    }

    public async Task<Result<Maybe<LocalMediaServerFiles>>> IntegrateMediaSetToLocalInfuseMediaLibrary(MediaSet mediaSet)
    {
        _logger.LogInformation("Integriere Medienset in die Infuse-Mediathek.");

        // Berücksichtige bei den Bilder nur diejenigen, die im Adobe RGB-Farbraum sind.
        var supportedImages = mediaSet.ImageFiles.Value.Where(image => image.IsAdobeRgbColorSpace).ToList();
        var internetStreamingVideoFiles = mediaSet.InternetStreamingVideoFiles.Value;
        if (supportedImages.Count != 0 || internetStreamingVideoFiles.Count != 0)
        {
            _logger.LogInformation("Folgende Dateien werden berücksichtigt aus dem Medienset für die Integration in die Infuse-Mediathek:");
            foreach (var supportedImage in supportedImages)
            {
                _logger.LogInformation("Bild: {supportedImage.FileInfo.FullName}", supportedImage.FileInfo.FullName);
            }
            foreach (var supportedImage in supportedImages)
            {
                _logger.LogInformation("Bild: {supportedImage.FileInfo.FullName}", supportedImage.FileInfo.FullName);
            }
        }
        else
        {
            _logger.LogInformation("Keine Dateien für die Integration in die Infuse-Mediathek vorhanden.");
            return Maybe<LocalMediaServerFiles>.None;
        }

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

        _logger.LogInformation("Integriere Medienset in die Infuse-Mediathek.");

        if (mediaSet == null)
            return Result.Failure<Maybe<LocalMediaServerFiles>>("Das Medienset ist null.");

        _logger.LogInformation("Prüfe ob im Medienset {mediaSet.Title} Medien für lokale Medienserver vorhanden sind.", mediaSet.Title);
        if (mediaSet.LocalMediaServerVideoFile.HasNoValue)
        {
            _logger.LogInformation("Keine Videos für lokale Medienserver im Medienset {mediaSet.Title} vorhanden.", mediaSet.Title);
            _logger.LogInformation("Überspringe Integration in die Infuse-Mediathek für dieses Medienset.");
            return Maybe<LocalMediaServerFiles>.None;
        }

        // Ermittle das Album aus den Metadaten der Video-Datei
        var albumResult = await _ffmpegMetadataService.GetMetadataFieldAsync(mediaSet.LocalMediaServerVideoFile.Value.FileInfo, "album");
        if (albumResult.IsFailure)
        {
            return Result.Failure<Maybe<LocalMediaServerFiles>>($"Das Album konnte nicht aus den Metadaten der Video-Datei {mediaSet.LocalMediaServerVideoFile.Value.FileInfo.Name} ermittelt werden: {albumResult.Error}");
        }
        Maybe<string> album = string.IsNullOrWhiteSpace(albumResult.Value) ? Maybe<string>.None : albumResult.Value;
        if (album.HasNoValue)
        {
            _logger.LogTrace("Album-Tag ist nicht in den Metadaten der Video-Datei {FileInfo.Name} vorhanden.", mediaSet.LocalMediaServerVideoFile.Value.FileInfo.Name);
            _logger.LogTrace("Das Album wird für die Integration in die Infuse-Mediathek nicht verwendet.");
        }
        else
        {
            _logger.LogTrace("Album-Tag aus den Metadaten der Video-Datei {FileInfo.Name} ermittelt: {album.Value}", mediaSet.LocalMediaServerVideoFile.Value.FileInfo.Name, album.Value);
            _logger.LogTrace($"Das Album wird für die Integration in die Infuse-Mediathek als erste Verzeichnisebene verwendet.");
        }

        // Ermittle das Aufnahmedatum aus dem Titel der Video-Datei. Das Aufnahemdatum ist als ISO-String im Titel enthalten mit einem Leerzeichen getrennt.
        var recordingDate = GetRecordingDateFromTitle(mediaSet.Title);
        if (recordingDate.HasNoValue)
        {
            _logger.LogTrace("Das Aufnahmedatum konnte nicht aus dem Titel der Video-Datei {FileInfo.Name} ermittelt werden.", mediaSet.LocalMediaServerVideoFile.Value.FileInfo.Name);
            _logger.LogTrace("Das Aufnahmedatum wird für die Integration in die Infuse-Mediathek nicht verwendet.");
        }
        else
        {
            _logger.LogTrace("Aufnahmedatum aus dem Titel der Video-Datei {FileInfo.Name} ermittelt: {recordingDate.Value}", mediaSet.LocalMediaServerVideoFile.Value.FileInfo.Name, recordingDate.Value);
            _logger.LogTrace("Das Aufnahmedatum wird für die Integration in die Infuse-Mediathek als zweite Verzeichnisebene verwendet.");
        }

        _logger.LogInformation("Gefunden in den Metadaten der Video-Datei:");
        _logger.LogInformation("Album: {album}", album);
        _logger.LogInformation("Aufnahmedatum: {recordingDate}", recordingDate);

        var targetFilePathResult = GetTargetFilePath(mediaSet.LocalMediaServerVideoFile.Value, album.Value, mediaSet.Title, recordingDate.Value);
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
        _logger.LogInformation("Verschiebe Video-Datei {FileInfo.FullName} in das Infuse-Mediathek-Verzeichnis {targetDirectory.FullName}", mediaSet.LocalMediaServerVideoFile.Value.FileInfo.FullName, targetDirectory.FullName);
        var moveFileResult = await _fileOperations.CopyFileAsync(mediaSet.LocalMediaServerVideoFile.Value.FileInfo.FullName, targetFilePathResult.Value.FullName, true);
        if (moveFileResult.IsFailure)
        {
            return Result.Failure<Maybe<LocalMediaServerFiles>>($"Die Video-Datei {mediaSet.LocalMediaServerVideoFile.Value.FileInfo.FullName} konnte nicht in das Infuse-Mediathek-Verzeichnis {targetDirectory.FullName} verschoben werden. Fehler: {moveFileResult.Error}");
        }
        _logger.LogInformation("Video-Datei {FileInfo.FullName} erfolgreich in das Infuse-Mediathek-Verzeichnis {targetDirectory.FullName} verschoben.", mediaSet.LocalMediaServerVideoFile.Value.FileInfo.FullName, targetDirectory.FullName);

        // Integriere die Bild-Dateien in das Infuse-Mediathek-Verzeichnis. Diese haben den gleichen Namen und das gleiche Zielverzeichnis wie die Video-Datei.
        var movedSupportedImagesResult = await IntegratedSupportedImagesToInfuseMediaLibrary(supportedImages, targetFilePathResult.Value);
        if (movedSupportedImagesResult.IsFailure)
        {
            _logger.LogWarning("Die Bild-Dateien konnten nicht in das Infuse-Mediathek-Verzeichnis {targetDirectory.FullName} verschoben werden: {movedSupportedImagesResult.Error}", targetDirectory.FullName, movedSupportedImagesResult.Error);
            _logger.LogInformation("Es werden keine Bild-Dateien in das Infuse-Mediathek-Verzeichnis verschoben.");
        }

        // Erstelle neues LocalMediaServerFiles-Objekt mit der verschobenen Video-Datei
        var movedSupportedVideo = SupportedVideo.Create(targetFilePathResult.Value);
        if (movedSupportedVideo.IsFailure)
        {
            return Result.Failure<Maybe<LocalMediaServerFiles>>($"Die verschobene Video-Datei {targetFilePathResult.Value.FullName} konnte nicht als SupportedVideo-Objekt erstellt werden: {movedSupportedVideo.Error}");
        }

        var localMediaServerFiles = new LocalMediaServerFiles(mediaSet.ImageFiles.Value, movedSupportedVideo.Value);
        return Maybe<LocalMediaServerFiles>.From(localMediaServerFiles);
    }

    /// <summary>
    /// Verschiebt die unterstützten Bild-Dateien in das Infuse-Mediathek-Verzeichnis und konvertiert den Farbraum der Bilder in Adobe RGB.
    /// </summary>
    /// <param name="supportedImages"></param>
    /// <param name="videoFileTargetPath"></param>
    /// <returns></returns>
    private async Task<Result> IntegratedSupportedImagesToInfuseMediaLibrary(IEnumerable<SupportedImage> supportedImages, FileInfo videoFileTargetPath)
    {
        // Wenn kein Bild vorhanden sind, wird mit einer Info geloggt und die Methode beendet.
        if (!supportedImages.Any())
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
        if (supportedImages.Count() == 1)
        {
            var supportedImage = supportedImages.First();

            var targetFilePath = Path.Combine(videoTargetDirectory.FullName, videoFileTargetPath.Name.Replace(videoFileTargetPath.Extension, supportedImage.FileInfo.Extension));
            var moveFileResult = await _fileOperations.CopyFileAsync(supportedImage.FileInfo.FullName, targetFilePath);
            if (moveFileResult.IsFailure)
            {
                return Result.Failure($"Die Bild-Datei {supportedImage.FileInfo.FullName} konnte nicht in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben werden. Fehler: {moveFileResult.Error}");
            }
            _logger.LogInformation("Bild-Datei {supportedImage.FileInfo.FullName} erfolgreich in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben.", supportedImage.FileInfo.FullName, videoTargetDirectory.FullName);
            return Result.Success();
        }

        // Wenn mehr als ein Bild vorhanden ist, dann werden die ersten zwei Bilder als Poster und Fanart verwendet und mit Hilfe des PosterAndFanartService die passenden Bilder ermittelt.
        var detectPosterAndFanartImagesResult = _posterAndFanartService.DetectPosterAndFanartImages(supportedImages.ElementAt(0), supportedImages.ElementAt(1));
        if (detectPosterAndFanartImagesResult.IsFailure)
        {
            return Result.Failure($"Das Poster und Fanart konnte nicht ermittelt werden: {detectPosterAndFanartImagesResult.Error}");
        }
        _logger.LogInformation($"Das Poster und Fanart wurde erfolgreich ermittelt.");
        _logger.LogInformation("Poster: {detectPosterAndFanartImagesResult.Value.PosterImage.FileInfo.FullName}", detectPosterAndFanartImagesResult.Value.PosterImage.FileInfo.FullName);
        _logger.LogInformation("Fanart: {detectPosterAndFanartImagesResult.Value.FanartImage.FileInfo.FullName}", detectPosterAndFanartImagesResult.Value.FanartImage.FileInfo.FullName);

        // Das Posterbild hat den gleichen Dateinamen die Videodatei.
        var posterImage = detectPosterAndFanartImagesResult.Value.PosterImage;
        var targetPosterFilePath = Path.Combine(videoTargetDirectory.FullName, videoFileTargetPath.Name.Replace(videoFileTargetPath.Extension, posterImage.FileInfo.Extension));
        var movePosterFileResult = await _fileOperations.CopyFileAsync(posterImage.FileInfo.FullName, targetPosterFilePath);
        if (movePosterFileResult.IsFailure)
        {
            return Result.Failure($"Das Posterbild {posterImage.FileInfo.FullName} konnte nicht in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben werden. Fehler: {movePosterFileResult.Error}");
        }
        _logger.LogInformation("Posterbild {posterImage.FileInfo.FullName} erfolgreich in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben.", posterImage.FileInfo.FullName, videoTargetDirectory.FullName);

        // Das Fanartbild hat den gleichen Dateinamen wie die Videodatei und zusätzlich dem Postfix definiert aus den Einstellungen.
        var bannerFilePostfix = _infuseMediaLibrarySettings.BannerFilePostfix;
        if (string.IsNullOrWhiteSpace(bannerFilePostfix))
        {
            return Result.Failure("Das Suffix des Dateinamens, das für die Banner-Datei verwendet wird für die Infuse-Mediathek als Titelbild, ist nicht definiert.");
        }
        var fanartImage = detectPosterAndFanartImagesResult.Value.FanartImage;
        var targetFanartFilePath = Path.Combine(videoTargetDirectory.FullName, videoFileTargetPath.Name.Replace(videoFileTargetPath.Extension, $"{bannerFilePostfix}{fanartImage.FileInfo.Extension}"));
        var moveFanartFileResult = await _fileOperations.CopyFileAsync(fanartImage.FileInfo.FullName, targetFanartFilePath);
        if (moveFanartFileResult.IsFailure)
        {
            return Result.Failure($"Das Fanartbild {fanartImage.FileInfo.FullName} konnte nicht in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben werden. Fehler: {moveFanartFileResult.Error}");
        }
        _logger.LogInformation("Fanartbild {fanartImage.FileInfo.FullName} erfolgreich in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben.", fanartImage.FileInfo.FullName, videoTargetDirectory.FullName);

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
    private Result<FileInfo> GetTargetFilePath(SupportedVideo supportedVideo, string? album, string? title, DateOnly recordingDate)
    {
        if (string.IsNullOrWhiteSpace(album))
            return Result.Failure<FileInfo>("Das Album ist leer.");
        
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<FileInfo>("Der Titel ist leer.");

        if (supportedVideo.FileInfo == null)
            return Result.Failure<FileInfo>("Die Quelldatei des SupportedVideo-Objekts ist null.");

        var targetDirectory = Path.Combine(_applicationSettings.InfuseMediaLibraryPathLocal, album, recordingDate.Year.ToString(), title);

        // Der Ziel-Dateiname ist ohne vorangestelltes ISO-Datum. Dieses muss also aus dem Titel entfernt werden.
        var titleWithoutLeadingRecordingDate = title.Replace($"{recordingDate:yyyy-MM-dd} ", string.Empty);

        var targetFileName = $"{titleWithoutLeadingRecordingDate}{supportedVideo.FileInfo.Extension}";
        var targetFilePath = Path.Combine(targetDirectory, targetFileName);
    
        return new FileInfo(targetFilePath);
    }
}

public record IntegratedMediaSetFile
{
    /// <summary>
    /// Die Quelldatei, die in das Infuse-Mediathek-Verzeichnis integriert wurde.
    /// </summary>
    public FileInfo SourceFile { get; }

    /// <summary>
    /// Die Zieldatei im Infuse-Mediathek-Verzeichnis.
    /// </summary>
    public FileInfo TargetFile { get; }

    public IntegratedMediaSetFile(FileInfo sourceFile, FileInfo targetFile)
    {
        SourceFile = sourceFile;
        TargetFile = targetFile;
    }
}