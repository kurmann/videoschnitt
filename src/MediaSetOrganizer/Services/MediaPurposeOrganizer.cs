using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.ConfigurationModule.Services;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;

namespace Kurmann.Videoschnitt.MediaSetOrganizer.Services;

/// <summary>
/// Organisiert die Mediendatien in einem Medienset nach Einsatzzweck (Medienserver oder Internet) 
/// anhand von Videovarienten-Suffixen der Medienset-Einstellungen.
/// </summary>
public class MediaPurposeOrganizer
{
    private readonly ILogger<MediaPurposeOrganizer> _logger;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;

    public MediaPurposeOrganizer(ILogger<MediaPurposeOrganizer> logger, IConfigurationService configurationService)
    {
        _logger = logger;
        _mediaSetOrganizerSettings = configurationService.GetSettings<MediaSetOrganizerSettings>();
    }

    /// <summary>
    /// Organisiert die Medienset-Verzeichnisse nach Einsatzzweck (Medienserver oder Internet).
    /// </summary>
    /// <param name="mediaFilesByMediaSets"></param>
    /// <returns></returns>
    public Result<List<MediaSet>> OrganizeMediaByPurpose(IEnumerable<MediaFilesByMediaSet> mediaFilesByMediaSets)
    {
        _logger.LogInformation("Versuche die Medienset-Verzeichnisse nach Einsatzzweck zu organisieren.");
        var mediaSetDirectoriesWithMediaPurpose = new List<MediaSet>();
        foreach (var mediaFilesByMediaSet in mediaFilesByMediaSets)
        {
            _logger.LogTrace("Organisiere Medienset-Verzeichnis '{Title}' nach Einsatzzweck.", mediaFilesByMediaSet.Title);

            var imageFiles = mediaFilesByMediaSet.ImageFiles.Any() ? Maybe<List<SupportedImage>>.From(mediaFilesByMediaSet.ImageFiles.ToList()) : Maybe<List<SupportedImage>>.None;

            var localMediaServerFiles = GetVideoForLocalMediaServer(mediaFilesByMediaSet);
            if (localMediaServerFiles.IsFailure)
            {
                return Result.Failure<List<MediaSet>>(localMediaServerFiles.Error);
            }

            var internetStreamingFiles = GetFilesForInternetStreaming(mediaFilesByMediaSet);
            if (internetStreamingFiles.IsFailure)
            {
                return Result.Failure<List<MediaSet>>(internetStreamingFiles.Error);
            }

            _logger.LogInformation("Medienset-Verzeichnis '{Title}' wurde erfolgreich nach Einsatzzweck organisiert.", mediaFilesByMediaSet.Title);
            var mediaSet = new MediaSet{
                Title = mediaFilesByMediaSet.Title, 
                LocalMediaServerVideoFile = localMediaServerFiles.Value, 
                InternetStreamingVideoFiles = internetStreamingFiles.Value, 
                ImageFiles = imageFiles};
            _logger.LogInformation("Das Medienset mt dem Titel '{Title}' enthält folgende Dateien nach Einsatzzweck", mediaFilesByMediaSet.Title);
            _logger.LogInformation("Lokaler Medienserver: {LocalMediaServerVideoFile}", localMediaServerFiles.Value.HasValue ? localMediaServerFiles.Value.Value.FileInfo.Name : "Keine Datei");
            _logger.LogInformation("Internet-Streaming: {InternetStreamingVideoFiles}", internetStreamingFiles.Value.HasValue ? string.Join(", ", internetStreamingFiles.Value.Value.Select(videoFile => videoFile.FileInfo.Name)) : "Keine Datei");
            _logger.LogInformation("Bilder: {ImageFiles}", imageFiles.HasValue ? string.Join(", ", imageFiles.Value.Select(imageFile => imageFile.FileInfo.Name)) : "Keine Datei");
            mediaSetDirectoriesWithMediaPurpose.Add(mediaSet);
        }

        _logger.LogInformation("Medienset-Verzeichnisse wurden erfolgreich nach Einsatzzweck organisiert.");
        return mediaSetDirectoriesWithMediaPurpose;
    }

    private Result<Maybe<SupportedVideo>> GetVideoForLocalMediaServer(MediaFilesByMediaSet mediaFilesByMediaSets)
    {
        _logger.LogTrace($"Filtere alle Videodateien, die mit einem der Suffixe für den Medienserver enden.");

        if (_mediaSetOrganizerSettings.MediaSet?.VideoVersionSuffixesForMediaServer == null)
        {
            return Result.Failure<Maybe<SupportedVideo>>("Medienset-Einstellungen wurden nicht korrekt geladen. Es kann keine Unterteilung in lokale Medienserver-Daten durchgeführt werden.");
        }

        var videoFilesForMediaServer = new List<SupportedVideo>();
        foreach (var videoFile in mediaFilesByMediaSets.VideoFiles)
        {
            if (_mediaSetOrganizerSettings.MediaSet.VideoVersionSuffixesForMediaServer.Any(suffix => videoFile.FileInfo.Name.Contains(suffix)))
            {
                videoFilesForMediaServer.Add(videoFile);
            }
        }
        _logger.LogTrace($"Prüfe, ob maximal ein Videodatei für den Medienserver vorhanden ist.");
        if (videoFilesForMediaServer.Count > 1)
        {
            return Result.Failure<Maybe<SupportedVideo>>("Es wurde mehr als eine Videodatei für den Medienserver gefunden. Es darf maximal eine Videodatei für den Medienserver vorhanden sein.");
        }
        if (videoFilesForMediaServer.Count == 1)
        {
            var videoFile = videoFilesForMediaServer.First();
            _logger.LogInformation("Es wurde die Videodatei '{videoFile}' für den Medienserver im Medienset-Verzeichnis '{Title}' gefunden.", videoFile.FileInfo.Name, mediaFilesByMediaSets.Title);
            return Maybe<SupportedVideo>.From(videoFile);
        }
        return Maybe<SupportedVideo>.None;
    }

    private Result<Maybe<List<SupportedVideo>>> GetFilesForInternetStreaming(MediaFilesByMediaSet mediaFilesByMediaSets)
    {
        _logger.LogTrace($"Filtere alle Videodateien, die mit einem der Suffixe für das Internet enden.");
        if (_mediaSetOrganizerSettings.MediaSet?.VideoVersionSuffixesForInternet == null)
        {
            return Result.Failure<Maybe<List<SupportedVideo>>>("Medienset-Einstellungen wurden nicht korrekt geladen. Es kann keine Unterteilung in Internet-Streaming-Daten durchgeführt werden.");
        }

        var videoFilesForInternet = new List<SupportedVideo>();
        foreach (var videoFile in mediaFilesByMediaSets.VideoFiles)
        {
            if (_mediaSetOrganizerSettings.MediaSet.VideoVersionSuffixesForInternet.Any(suffix => videoFile.FileInfo.Name.Contains(suffix)))
            {
                videoFilesForInternet.Add(videoFile);
            }
        }
        
        if (videoFilesForInternet.Count == 0)
        {
            _logger.LogInformation("Es wurden keine Videodateien für das Internet im Medienset-Verzeichnis '{Title}' gefunden.", mediaFilesByMediaSets.Title);
            return Maybe<List<SupportedVideo>>.None;
        }

        var videoFileNames = string.Join(", ", videoFilesForInternet.Select(videoFile => videoFile.FileInfo.Name));
        _logger.LogInformation("Es wurden die Videodateien '{videoFileNames}' für das Internet im Medienset-Verzeichnis '{Title}' gefunden.", videoFileNames, mediaFilesByMediaSets.Title);

        return Maybe<List<SupportedVideo>>.From(videoFilesForInternet);
    }
}
