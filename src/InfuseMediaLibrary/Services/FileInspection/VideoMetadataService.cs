using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Services.Metadata;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;

public class VideoMetadataService
{
    private readonly ILogger<VideoMetadataService> _logger;
    private readonly FFmpegMetadataService _ffmpegMetadataService;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;

    public VideoMetadataService(ILogger<VideoMetadataService> logger, FFmpegMetadataService ffmpegMetadataService, 
        IOptions<MediaSetOrganizerSettings> mediaSetOrganizerSettings)
    {
        _logger = logger;
        _ffmpegMetadataService = ffmpegMetadataService;
        _mediaSetOrganizerSettings = mediaSetOrganizerSettings.Value;
    }

    /// <summary>
    /// Entnimmt das Album aus den Metadaten der Video-Datei.
    /// </summary>
    /// <returns></returns>
    public async Task<Result<string>> GetAlbumAsync(FileInfo videoFile)
    {
        // Ermittle das Album aus den Metadaten der Video-Datei
        var albumResult = await _ffmpegMetadataService.GetMetadataFieldAsync(videoFile, "album");
        if (albumResult.IsFailure)
        {
            return Result.Failure<string>($"Das Album konnte nicht aus den Metadaten der Video-Datei {videoFile} ermittelt werden: {albumResult.Error}");
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

        return Result.Success(album.Value);
    }

    /// <summary>
    /// Ermittelt aus dem Verzeichnisbaum der Video-Datei den Titel des Mediensets.
    /// </summary>
    /// <param name="videoFile"></param>
    /// <returns></returns>
    public Result<string> GetTitle(FileInfo videoFile)
    {
        var videoFileDirectory = videoFile.Directory;
        if (videoFileDirectory == null)
        {
            _logger.LogWarning("Das Verzeichnis der Video-Datei {FileInfo.Name} konnte nicht ermittelt werden.", videoFile.Name);
            return Result.Failure<string>("Das Verzeichnis der Video-Datei konnte nicht ermittelt werden.");
        }

        // Es wird angenommen, dass der Titel des Mediensets wie folgt aufgebaut ist <RootDirectory>/<MediasetName>/<MediaServerFilesSubDirectoryName>/<VideoFile>
        var mediaServerFilesDirectoryDefinedBySettings = _mediaSetOrganizerSettings.MediaSet.MediaServerFilesSubDirectoryName;

        // Ermittle den Names des Verzeichnisses oberhalb des Verzeichnisses für die Media-Server-Dateien. Also das Eltern-Verzeichnis des Verzeichnisses für die Media-Server-Dateien.
        var mediaServerFilesDirectoryName = videoFileDirectory.Parent?.Name;

        // Prüfe ob das Elternverzeichnis der Videodatei den erwarteten Namen für das Verzeichnis der Media-Server-Dateien hat.
        if (mediaServerFilesDirectoryName != mediaServerFilesDirectoryDefinedBySettings)
        {
            return Result.Failure<string>($"Das Elternverzeichnis der Video-Datei hat nicht den erwarteten Namen für Verzeichnis der Media-Server-Dateien: {mediaServerFilesDirectoryDefinedBySettings}.");
        }

        // Der Name des Verzeichnisses des Mediensets ist das übergeordnete Verzeichnis des Verzeichnisses für die Media-Server-Dateien.
        var mediaSetDirectoryName = videoFileDirectory.Parent?.Parent?.Name;
        if (string.IsNullOrWhiteSpace(mediaSetDirectoryName))
        {
            return Result.Failure<string>("Der Name des Verzeichnisses des Mediensets konnte nicht ermittelt werden.");
        }

        // Der Name des Mediensets ist der Name des Verzeichnisses des Mediensets.
        return Result.Success(mediaSetDirectoryName);
    }

    /// <summary>
    /// Ermittelt das Aufnahmedatum aus dem Titel der Video-Datei.
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    public Result<DateOnly> GetRecordingDate(string title)
    {
        // Ermittle das Aufnahmedatum aus dem Titel der Video-Datei. Das Aufnahemdatum ist als ISO-String im Titel enthalten mit einem Leerzeichen getrennt.
        var recordingDate = GetRecordingDateFromTitle(title);
        if (recordingDate.HasNoValue)
        {
            _logger.LogTrace("Das Aufnahmedatum konnte nicht aus dem Titel der Video-Datei ermittelt werden.");
            _logger.LogTrace("Das Aufnahmedatum wird für die Integration in die Infuse-Mediathek nicht verwendet.");
        }
        else
        {
            _logger.LogTrace("Aufnahmedatum aus dem Titel der Video-Datei ermittelt: {recordingDate}", recordingDate.Value);
            _logger.LogTrace("Das Aufnahmedatum wird für die Integration in die Infuse-Mediathek als zweite Verzeichnisebene verwendet.");
        }

        return Result.Success(recordingDate.Value);
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
}
