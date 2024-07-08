using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.MediaSetOrganizer.Services;

/// <summary>
/// Verantwortlich für die Integration von Medienset-Verzeichnissen.
/// </summary>
public class MediaSetDirectoryIntegrator
{
    public readonly ApplicationSettings _applicationSettings;
    public readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;
    private readonly ILogger<MediaSetDirectoryIntegrator> _logger;
    private readonly IFileOperations _fileOperations;

    public MediaSetDirectoryIntegrator(IOptions<ApplicationSettings> applicationSettings,
                                       IOptions<MediaSetOrganizerSettings> mediaSetOrganizerSettings,
                                       ILogger<MediaSetDirectoryIntegrator> logger,
                                       IFileOperations fileOperations)
    {
        _applicationSettings = applicationSettings.Value;
        _mediaSetOrganizerSettings = mediaSetOrganizerSettings.Value;
        _logger = logger;
        _fileOperations = fileOperations;
    }

    public async Task<Result<List<DirectoryInfo>>> IntegrateInLocalMediaSetDirectory(IEnumerable<MediaSet> mediaSets)
    {
        _logger.LogInformation("Integriere Mediensets in lokales Medienset-Verzeichnis.");
        _logger.LogInformation("Berücksichtige den Einsatzzweck der Medien indem diese in ein vordefiniertes Unterverzeichnis verschoben werden.");
        _logger.LogInformation("Unterverzeichnis für Medienserver: {mediaServerFilesSubDirectoryName}", _mediaSetOrganizerSettings.MediaSet.MediaServerFilesSubDirectoryName);
        _logger.LogInformation("Unterverzeichnis für Internet: {internetFilesSubDirectoryName}", _mediaSetOrganizerSettings.MediaSet.InternetFilesSubDirectoryName);
        var mediaSetDirectories = new List<DirectoryInfo>();

        foreach (var mediaSet in mediaSets)
        {
            if (mediaSet.Title == null)
            {
                return Result.Failure<List<DirectoryInfo>>("Medienset-Titel ist null.");
            }

            var mediaSetTargetDirectory = new DirectoryInfo(Path.Combine(_applicationSettings.MediaSetPathLocal, mediaSet.Title));
            if (!mediaSetTargetDirectory.Exists)
            {
                _logger.LogInformation("Erstelle Medienset-Verzeichnis: {mediaSetDirectory}", mediaSetTargetDirectory.FullName);
                var directoryCreateResult = await _fileOperations.CreateDirectoryAsync(mediaSetTargetDirectory.FullName);
                if (directoryCreateResult.IsFailure)
                {
                    return Result.Failure<List<DirectoryInfo>>($"Fehler beim Erstellen des Medienset-Verzeichnisses: {directoryCreateResult.Error}");
                }
            }

            _logger.LogInformation("Verschiebe Medien-Dateien in das Medienset-Verzeichnis: {mediaSetDirectory}", mediaSetTargetDirectory.FullName);
            if (mediaSet.LocalMediaServerVideoFile.HasNoValue)
            {
                _logger.LogInformation("Keine Medien-Dateien für den Medienserver vorhanden.");
            }
            else
            {
                var videoFileForMediaServer = mediaSet.LocalMediaServerVideoFile.Value;
                var videoFileForMediaServerTargetPath = Path.Combine(mediaSetTargetDirectory.FullName,
                                                                     _mediaSetOrganizerSettings.MediaSet.MediaServerFilesSubDirectoryName,
                                                                     videoFileForMediaServer.FileInfo.Name);
                _logger.LogInformation("Verschiebe Video-Datei für Medienserver: {videoFileForMediaServerTargetPath}", videoFileForMediaServerTargetPath);

                // Prüfe ob das Unterzielverzeichnis für die Medienserver-Dateien existiert
                var mediaServerFilesSubDirectory = new DirectoryInfo(Path.Combine(mediaSetTargetDirectory.FullName, _mediaSetOrganizerSettings.MediaSet.MediaServerFilesSubDirectoryName));
                if (!mediaServerFilesSubDirectory.Exists)
                {
                    _logger.LogInformation("Erstelle Unterverzeichnis für Medienserver-Dateien: {mediaServerFilesSubDirectory}", mediaServerFilesSubDirectory.FullName);
                    var mediaServerFilesSubDirectoryCreateResult = await _fileOperations.CreateDirectoryAsync(mediaServerFilesSubDirectory.FullName);
                    if (mediaServerFilesSubDirectoryCreateResult.IsFailure)
                    {
                        return Result.Failure<List<DirectoryInfo>>($"Fehler beim Erstellen des Unterverzeichnisses für Medienserver-Dateien: {mediaServerFilesSubDirectoryCreateResult.Error}");
                    }
                }

                var videoFileForMediaServerMoveResult = await _fileOperations.MoveFileAsync(videoFileForMediaServer.FileInfo.FullName, videoFileForMediaServerTargetPath, true);
                if (videoFileForMediaServerMoveResult.IsFailure)
                {
                    return Result.Failure<List<DirectoryInfo>>($"Fehler beim Verschieben der Video-Datei für den Medienserver: {videoFileForMediaServerMoveResult.Error}");
                }
                _logger.LogInformation("Video-Datei für Medienserver erfolgreich verschoben.");
            }

            mediaSetDirectories.Add(mediaSetTargetDirectory);
        }

        return Result.Success(mediaSetDirectories);
    }
}
