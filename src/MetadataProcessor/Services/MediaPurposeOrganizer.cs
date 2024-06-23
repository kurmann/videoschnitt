using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

/// <summary>
/// Organisiert die Mediendatien in einem Medienset nach Einsatzzweck (Medienserver oder Internet) 
/// anhand von Videovarienten-Suffixen der Medienset-Einstellungen.
/// </summary>
public class MediaPurposeOrganizer
{
    private readonly ILogger<MediaPurposeOrganizer> _logger;
    private readonly ModuleSettings _moduleSettings;

    public MediaPurposeOrganizer(ILogger<MediaPurposeOrganizer> logger, IOptions<ModuleSettings> moduleSettings)
    {
        _logger = logger;
        _moduleSettings = moduleSettings.Value;
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
            _logger.LogTrace($"Organisiere Medienset-Verzeichnis '{mediaFilesByMediaSet.Title}' nach Einsatzzweck.");

            var localMediaServerFiles = GetFilesForLocalMediaServer(mediaFilesByMediaSet);
            if (localMediaServerFiles.IsFailure)
            {
                return Result.Failure<List<MediaSet>>(localMediaServerFiles.Error);
            }

            var internetStreamingFiles = GetFilesForInternetStreaming(mediaFilesByMediaSet);
            if (internetStreamingFiles.IsFailure)
            {
                return Result.Failure<List<MediaSet>>(internetStreamingFiles.Error);
            }

            _logger.LogInformation($"Medienset-Verzeichnis '{mediaFilesByMediaSet.Title}' wurde erfolgreich nach Einsatzzweck organisiert.");
            mediaSetDirectoriesWithMediaPurpose.Add(new MediaSet(mediaFilesByMediaSet.Title, localMediaServerFiles.Value, internetStreamingFiles.Value));
        }

        _logger.LogInformation("Medienset-Verzeichnisse wurden erfolgreich nach Einsatzzweck organisiert.");
        return mediaSetDirectoriesWithMediaPurpose;
    }

    private Result<Maybe<LocalMediaServerFiles>> GetFilesForLocalMediaServer(MediaFilesByMediaSet mediaFilesByMediaSets)
    {
        _logger.LogTrace($"Filtere alle Videodateien, die mit einem der Suffixe für den Medienserver enden.");

        if (_moduleSettings.MediaSet?.VideoVersionSuffixesForMediaServer == null)
        {
            return Result.Failure<Maybe<LocalMediaServerFiles>>("Medienset-Einstellungen wurden nicht korrekt geladen. Es kann keine Unterteilung in Medienserver-Daten durchgeführt werden.");
        }

        var localMediaServerFiles = Maybe<LocalMediaServerFiles>.None;
        var videoFilesForMediaServer = new List<SupportedVideo>();
        foreach (var videoFile in mediaFilesByMediaSets.VideoFiles)
        {
            if (_moduleSettings.MediaSet.VideoVersionSuffixesForMediaServer.Any(suffix => videoFile.FileInfo.Name.Contains(suffix)))
            {
                videoFilesForMediaServer.Add(videoFile);
            }
        }
        _logger.LogTrace($"Prüfe, ob maximal ein Videodatei für den Medienserver vorhanden ist.");
        if (videoFilesForMediaServer.Count > 1)
        {
            return Result.Failure<Maybe<LocalMediaServerFiles>>($"Es sind mehr als eine Videodatei für den Medienserver im Medienset-Verzeichnis '{mediaFilesByMediaSets.Title}' vorhanden.");
        }
        if (videoFilesForMediaServer.Count == 1)
        {
            var videoFile = videoFilesForMediaServer.First();
            localMediaServerFiles = new LocalMediaServerFiles(mediaFilesByMediaSets.ImageFiles, videoFile);
            _logger.LogInformation($"Es wurde die Videodatei '{videoFile}' für den Medienserver im Medienset-Verzeichnis '{mediaFilesByMediaSets.Title}' gefunden.");
        }

        if (localMediaServerFiles.HasNoValue)
        {
            _logger.LogInformation($"Es wurde keine Videodatei für den Medienserver im Medienset-Verzeichnis '{mediaFilesByMediaSets.Title}' gefunden.");
        }

        return localMediaServerFiles;
    }

    private Result<Maybe<InternetStreamingFiles>> GetFilesForInternetStreaming(MediaFilesByMediaSet mediaFilesByMediaSets)
    {
        _logger.LogTrace($"Filtere alle Videodateien, die mit einem der Suffixe für das Internet enden.");

        if (_moduleSettings.MediaSet?.VideoVersionSuffixesForInternet == null)
        {
            return Result.Failure<Maybe<InternetStreamingFiles>>("Medienset-Einstellungen wurden nicht korrekt geladen. Es kann keine Unterteilung in Internet-Daten durchgeführt werden.");
        }

        var internetStreaming = Maybe<InternetStreamingFiles>.None;
        var videoFilesForInternet = new List<SupportedVideo>();
        foreach (var videoFile in mediaFilesByMediaSets.VideoFiles)
        {
            if (_moduleSettings.MediaSet.VideoVersionSuffixesForInternet.Any(suffix => videoFile.FileInfo.Name.Contains(suffix)))
            {
                videoFilesForInternet.Add(videoFile);
            }
        }
        
        // Wenn keine Videodatei für das Internet gefunden wurde, dann wird ein leerer Maybe-Container zurückgegeben.
        if (videoFilesForInternet.Count == 0)
        {
            _logger.LogInformation($"Es wurden keine Videodateien für das Internet im Medienset-Verzeichnis '{mediaFilesByMediaSets.Title}' gefunden.");
            return Maybe<InternetStreamingFiles>.None;
        }

        internetStreaming = new InternetStreamingFiles(mediaFilesByMediaSets.ImageFiles, videoFilesForInternet);
        _logger.LogInformation($"Es wurden die Videodateien '{string.Join(", ", videoFilesForInternet.Select(v => v.FileInfo.Name))}' für das Internet im Medienset-Verzeichnis '{mediaFilesByMediaSets.Title}' gefunden.");

        return internetStreaming;
    }
}
