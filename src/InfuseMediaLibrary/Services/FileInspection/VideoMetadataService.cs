using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Services.Metadata;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;

internal class VideoMetadataService
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
    /// Ermittelt aus dem Verzeichnisbaum der Video-Datei den Namen des Mediensets.
    /// </summary>
    /// <param name="videoFile"></param>
    /// <returns></returns>
    public Result<MediaSetTitle> GetMediaSetName(FileInfo videoFile)
    {
        var videoFileDirectory = videoFile.Directory;
        if (videoFileDirectory == null)
        {
            _logger.LogWarning("Das Verzeichnis der Video-Datei {FileInfo.Name} konnte nicht ermittelt werden.", videoFile.Name);
            return Result.Failure<MediaSetTitle>("Das Verzeichnis der Video-Datei konnte nicht ermittelt werden.");
        }

        // Der Name des Mediensets ist der Name des Elternverzeichnisses des Verzeichnisses für die Media-Server-Dateien. 
        var parentDirectoryName = videoFileDirectory.Parent?.Name;
        if (string.IsNullOrWhiteSpace(parentDirectoryName))
        {
            return Result.Failure<MediaSetTitle>("Der Name des Elternverzeichnisses des Verzeichnisses für die Media-Server-Dateien konnte nicht ermittelt werden.");
        }

        // Parse den Verzeichnisnamen als Medienset-Namen
        var mediaSetTitle = MediaSetTitle.Create(parentDirectoryName);
        if (mediaSetTitle.IsFailure)
        {
            return Result.Failure<MediaSetTitle>($"Der Verzeichnisname {parentDirectoryName} konnte nicht als Medienset-Name geparst werden.");
        }

        return Result.Success(mediaSetTitle.Value);
    }
}
