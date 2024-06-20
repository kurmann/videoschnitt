using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;

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
    /// <param name="mediaSetDirectories"></param>
    /// <returns></returns>
    public Result<List<MediaSetDirectoryWithMediaPurpose>> OrganizeMediaByPurpose(IEnumerable<MediaSetDirectory> mediaSetDirectories)
    {
        _logger.LogInformation("Versuche die Medienset-Verzeichnisse nach Einsatzzweck zu organisieren.");
        var mediaSetDirectoriesWithMediaPurpose = new List<MediaSetDirectoryWithMediaPurpose>();
        foreach (var mediaSetDirectory in mediaSetDirectories)
        {
            _logger.LogTrace($"Organisiere Medienset-Verzeichnis '{mediaSetDirectory.MediaSetTitle}' nach Einsatzzweck.");

            var localMediaServerFiles = GetFilesForLocalMediaServer(mediaSetDirectory);
            if (localMediaServerFiles.IsFailure)
            {
                return Result.Failure<List<MediaSetDirectoryWithMediaPurpose>>(localMediaServerFiles.Error);
            }

            var internetStreaming = GetFilesForInternetStreaming(mediaSetDirectory);
            if (internetStreaming.IsFailure)
            {
                return Result.Failure<List<MediaSetDirectoryWithMediaPurpose>>(internetStreaming.Error);
            }

            _logger.LogInformation($"Medienset-Verzeichnis '{mediaSetDirectory.MediaSetTitle}' wurde erfolgreich nach Einsatzzweck organisiert.");
            mediaSetDirectoriesWithMediaPurpose.Add(new MediaSetDirectoryWithMediaPurpose(mediaSetDirectory.DirectoryInfo, mediaSetDirectory.MediaSetTitle, localMediaServerFiles.Value, internetStreaming.Value));
        }

        _logger.LogInformation("Medienset-Verzeichnisse wurden erfolgreich nach Einsatzzweck organisiert.");
        return mediaSetDirectoriesWithMediaPurpose;
    }

    private Result<Maybe<LocalMediaServerFiles>> GetFilesForLocalMediaServer(MediaSetDirectory mediaSetDirectory)
    {
        _logger.LogTrace($"Filtere alle Videodateien, die mit einem der Suffixe für den Medienserver enden.");

        if (_moduleSettings.MediaSet?.VideoVersionSuffixesForMediaServer == null)
        {
            return Result.Failure<Maybe<LocalMediaServerFiles>>("Medienset-Einstellungen wurden nicht korrekt geladen. Es kann keine Unterteilung in Medienserver-Daten durchgeführt werden.");
        }

        var localMediaServerFiles = Maybe<LocalMediaServerFiles>.None;
        var videoFilesForMediaServer = new List<SupportedVideo>();
        foreach (var videoFile in mediaSetDirectory.VideoFiles)
        {
            if (_moduleSettings.MediaSet.VideoVersionSuffixesForMediaServer.Any(suffix => videoFile.FileInfo.Name.EndsWith(suffix)))
            {
                videoFilesForMediaServer.Add(videoFile);
            }
        }
        _logger.LogTrace($"Prüfe, ob maximal ein Videodatei für den Medienserver vorhanden ist.");
        if (videoFilesForMediaServer.Count > 1)
        {
            return Result.Failure<Maybe<LocalMediaServerFiles>>($"Es sind mehr als eine Videodatei für den Medienserver im Medienset-Verzeichnis '{mediaSetDirectory.MediaSetTitle}' vorhanden.");
        }
        if (videoFilesForMediaServer.Count == 1)
        {
            var videoFile = videoFilesForMediaServer.First();
            localMediaServerFiles = new LocalMediaServerFiles(mediaSetDirectory.ImageFiles, videoFile);
            _logger.LogInformation($"Es wurde die Videodatei '{videoFile}' für den Medienserver im Medienset-Verzeichnis '{mediaSetDirectory.MediaSetTitle}' gefunden.");
        }

        if (localMediaServerFiles.HasNoValue)
        {
            _logger.LogInformation($"Es wurde keine Videodatei für den Medienserver im Medienset-Verzeichnis '{mediaSetDirectory.MediaSetTitle}' gefunden.");
        }

        return localMediaServerFiles;
    }

    private Result<Maybe<InternetStreaming>> GetFilesForInternetStreaming(MediaSetDirectory mediaSetDirectory)
    {
        _logger.LogTrace($"Filtere alle Videodateien, die mit einem der Suffixe für das Internet enden.");

        if (_moduleSettings.MediaSet?.VideoVersionSuffixesForInternet == null)
        {
            return Result.Failure<Maybe<InternetStreaming>>("Medienset-Einstellungen wurden nicht korrekt geladen. Es kann keine Unterteilung in Internet-Daten durchgeführt werden.");
        }

        var internetStreaming = Maybe<InternetStreaming>.None;
        var videoFilesForInternet = new List<SupportedVideo>();
        foreach (var videoFile in mediaSetDirectory.VideoFiles)
        {
            if (_moduleSettings.MediaSet.VideoVersionSuffixesForInternet.Any(suffix => videoFile.FileInfo.Name.EndsWith(suffix)))
            {
                videoFilesForInternet.Add(videoFile);
            }
        }
        _logger.LogTrace($"Prüfe, ob mindestens eine Videodatei für das Internet vorhanden ist.");
        if (videoFilesForInternet.Count == 0)
        {
            return Result.Failure<Maybe<InternetStreaming>>($"Es wurde keine Videodatei für das Internet im Medienset-Verzeichnis '{mediaSetDirectory.MediaSetTitle}' gefunden.");
        }

        internetStreaming = new InternetStreaming(mediaSetDirectory.ImageFiles, videoFilesForInternet);
        _logger.LogInformation($"Es wurden die Videodateien '{string.Join(", ", videoFilesForInternet.Select(v => v.FileInfo.Name))}' für das Internet im Medienset-Verzeichnis '{mediaSetDirectory.MediaSetTitle}' gefunden.");

        return internetStreaming;
    }
}

/// <summary>
/// Repräsentiert ein Medienset-Verzeichnis mit separierten Dateien für den Einsatzzweck auf einem lokalen Medienserver und im Internet.
/// Die jeweiligen Dateien für einen Einsatzzweck können auch leer sein, da nicht alle Mediensets für beide Einsatzzwecke konfiguriert sind
/// oder sich gewisse Dateien beim Verarbeitungsprozess noch nicht im Medienset-Verzeichnis befinden (bspw. während der Videokomprimierung).
/// </summary>
/// <param name="MediaSetDirectory"></param>
/// <param name="MediaSetTitle"></param>
/// <param name="LocalMediaServerFiles"></param>
/// <param name="InternetStreaming"></param>
/// <returns></returns>
public record MediaSetDirectoryWithMediaPurpose(DirectoryInfo MediaSetDirectory, string MediaSetTitle, Maybe<LocalMediaServerFiles> LocalMediaServerFiles, Maybe<InternetStreaming> InternetStreaming);

/// <summary>
/// Repräsentiert die Dateien eines Mediensets für die Wiedergabe über den lokalen Medienserver.
/// Hinweis: Auf dem lokalen Medienserver wird nur ein Video unterstützt.
/// Bilddateien können mehrere sein, da unterschiedliche Bilddateien als Bannerbild und als Thumbnail verwendet werden können.
/// </summary>
/// <param name="DirectoryInfo"></param>
/// <param name="MediaSetTitle"></param>
/// <param name="ImageFiles"></param>
/// <param name="VideoFile"></param>
/// <returns></returns>
public record LocalMediaServerFiles(IEnumerable<SupportedImage> ImageFiles, SupportedVideo VideoFile);

/// <summary>
/// Repräsentiert die Dateien eines Mediensets für das Internetwiedergabe als Teil eines Mediensets.
/// </summary>
/// <param name="DirectoryInfo"></param>
/// <param name="MediaSetTitle"></param>
/// <param name="ImageFiles"></param>
/// <param name="VideoFiles"></param>
/// <returns></returns>
public record InternetStreaming(IEnumerable<SupportedImage> ImageFiles, IEnumerable<SupportedVideo> VideoFiles);