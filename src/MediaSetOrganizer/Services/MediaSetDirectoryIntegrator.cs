using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Kurmann.Videoschnitt.MediaSetOrganizer.Services.Imaging;
using Kurmann.Videoschnitt.MediaSetOrganizer.Services.Integration;
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
    private readonly PortraitAndLandscapeService _portraitAndLandscapeService;
    private readonly SupportedImagesIntegrator _supportedImagesIntegrator;

    public MediaSetDirectoryIntegrator(IOptions<ApplicationSettings> applicationSettings,
                                       IOptions<MediaSetOrganizerSettings> mediaSetOrganizerSettings,
                                       ILogger<MediaSetDirectoryIntegrator> logger,
                                       IFileOperations fileOperations,
                                       PortraitAndLandscapeService portraitAndLandscapeService,
                                       SupportedImagesIntegrator supportedImagesIntegrator)
    {
        _applicationSettings = applicationSettings.Value;
        _mediaSetOrganizerSettings = mediaSetOrganizerSettings.Value;
        _logger = logger;
        _fileOperations = fileOperations;
        _portraitAndLandscapeService = portraitAndLandscapeService;
        _supportedImagesIntegrator = supportedImagesIntegrator;
    }

    public async Task<Result> IntegrateInLocalMediaSetDirectory(IEnumerable<MediaSet> mediaSets)
    {
        _logger.LogInformation("Integriere Mediensets in lokales Medienset-Verzeichnis.");
        _logger.LogInformation("Berücksichtige den Einsatzzweck der Medien indem diese in ein vordefiniertes Unterverzeichnis verschoben werden.");
        _logger.LogInformation("Unterverzeichnis für Medienserver: {mediaServerFilesSubDirectoryName}", _mediaSetOrganizerSettings.MediaSet.MediaServerFilesSubDirectoryName);
        _logger.LogInformation("Unterverzeichnis für Internet: {internetFilesSubDirectoryName}", _mediaSetOrganizerSettings.MediaSet.InternetFilesSubDirectoryName);
        _logger.LogInformation("Unterverzeichnis für Titelbilder: {imageFilesSubDirectoryName}", _mediaSetOrganizerSettings.MediaSet.ImageFilesSubDirectoryName);
        _logger.LogInformation("Unterverzeichnis für Masterdatei: {masterfileSubDirectoryName}", _mediaSetOrganizerSettings.MediaSet.MasterfileSubDirectoryName);

        foreach (var mediaSet in mediaSets)
        {
            if (mediaSet.Title == null)
            {
                return Result.Failure<List<MediaSet>>("Der Titel des Mediensets darf nicht leer sein.");
            }

            var mediaSetTargetDirectory = new DirectoryInfo(Path.Combine(_applicationSettings.MediaSetPathLocal, mediaSet.Title));
            if (!mediaSetTargetDirectory.Exists)
            {
                _logger.LogInformation("Erstelle Medienset-Verzeichnis: {mediaSetDirectory}", mediaSetTargetDirectory.FullName);
                var directoryCreateResult = await _fileOperations.CreateDirectoryAsync(mediaSetTargetDirectory.FullName);
                if (directoryCreateResult.IsFailure)
                {
                    return Result.Failure<List<MediaSet>>($"Fehler beim Erstellen des Medienset-Verzeichnisses: {directoryCreateResult.Error}");
                }
            }

            var integrateMediaServerFilesResult = await IntegrateMediaServerFiles(mediaSet, mediaSetTargetDirectory);
            if (integrateMediaServerFilesResult.IsFailure)
            {
                return Result.Failure<List<MediaSet>>($"Fehler beim Integrieren der Medienserver-Dateien: {integrateMediaServerFilesResult.Error}");
            }

            var integrateInternetFilesResult = await IntegrateInternetFiles(mediaSet, mediaSetTargetDirectory);
            if (integrateInternetFilesResult.IsFailure)
            {
                return Result.Failure<List<MediaSet>>($"Fehler beim Integrieren der Internet-Dateien: {integrateInternetFilesResult.Error}");
            }

            var inteagrateImageFilesResult = await _supportedImagesIntegrator.IntegrateImageFiles(mediaSet, mediaSetTargetDirectory);
            if (inteagrateImageFilesResult.IsFailure)
            {
                return Result.Failure<List<MediaSet>>($"Fehler beim Integrieren der Bild-Dateien: {inteagrateImageFilesResult.Error}");
            }

            var integratedMasterfileResult = await IntegrateMasterfile(mediaSet, mediaSetTargetDirectory);
            if (integratedMasterfileResult.IsFailure)
            {
                return Result.Failure<List<MediaSet>>($"Fehler beim Integrieren der Masterdatei: {integratedMasterfileResult.Error}");
            }
        }

        return Result.Success();
    }

    private async Task<Result> IntegrateMasterfile(MediaSet mediaSet, DirectoryInfo mediaSetTargetDirectory)
    {
        _logger.LogInformation("Verschiebe Masterdatei in das Medienset-Verzeichnis: {mediaSetDirectory}", mediaSetTargetDirectory.FullName);
        if (mediaSet.Masterfile.HasNoValue)
        {
            _logger.LogInformation("Keine Masterdatei für den Medienserver vorhanden.");
            return Result.Success(Maybe<Masterfile>.None);
        }
        else
        {
            var masterfileSubDirectory = new DirectoryInfo(Path.Combine(mediaSetTargetDirectory.FullName, _mediaSetOrganizerSettings.MediaSet.MasterfileSubDirectoryName));
            if (!masterfileSubDirectory.Exists)
            {
                _logger.LogInformation("Erstelle Unterverzeichnis für Masterdatei: {masterfileSubDirectory}", masterfileSubDirectory.FullName);
                var masterfileSubDirectoryCreateResult = await _fileOperations.CreateDirectoryAsync(masterfileSubDirectory.FullName);
                if (masterfileSubDirectoryCreateResult.IsFailure)
                {
                    return Result.Failure<Maybe<Masterfile>>($"Fehler beim Erstellen des Unterverzeichnisses für Masterdatei: {masterfileSubDirectoryCreateResult.Error}");
                }
            }

            var masterfileTargetPath = Path.Combine(masterfileSubDirectory.FullName, mediaSet.Masterfile.Value.FileInfo.Name);
            _logger.LogInformation("Verschiebe Masterdatei: {masterfileTargetPath}", masterfileTargetPath);
            var masterfileMoveResult = await _fileOperations.MoveFileAsync(mediaSet.Masterfile.Value.FileInfo.FullName, masterfileTargetPath, true);
            if (masterfileMoveResult.IsFailure)
            {
                return Result.Failure<Maybe<Masterfile>>($"Fehler beim Verschieben der Masterdatei: {masterfileMoveResult.Error}");
            }

            // Aktualisiere den Dateipfad der Masterdatei
            mediaSet.Masterfile = mediaSet.Masterfile.Value with { FileInfo = new FileInfo(masterfileTargetPath) };
            return Result.Success(Maybe<Masterfile>.From(mediaSet.Masterfile.Value));
        }
    }

    private async Task<Result> IntegrateInternetFiles(MediaSet mediaSet, DirectoryInfo mediaSetTargetDirectory)
    {
        _logger.LogInformation("Verschiebe Internet-Dateien in das Medienset-Verzeichnis: {mediaSetDirectory}", mediaSetTargetDirectory.FullName);
        if (mediaSet.InternetStreamingVideoFiles.HasNoValue)
        {
            _logger.LogInformation("Keine Internet-Dateien für den Medienserver vorhanden.");
            return Result.Success(new List<SupportedVideo>());
        }
        else
        {
            var internetFilesSubDirectory = new DirectoryInfo(Path.Combine(mediaSetTargetDirectory.FullName, _mediaSetOrganizerSettings.MediaSet.InternetFilesSubDirectoryName));
            if (!internetFilesSubDirectory.Exists)
            {
                _logger.LogInformation("Erstelle Unterverzeichnis für Internet-Dateien: {internetFilesSubDirectory}", internetFilesSubDirectory.FullName);
                var internetFilesSubDirectoryCreateResult = await _fileOperations.CreateDirectoryAsync(internetFilesSubDirectory.FullName);
                if (internetFilesSubDirectoryCreateResult.IsFailure)
                {
                    return Result.Failure<List<SupportedVideo>>($"Fehler beim Erstellen des Unterverzeichnisses für Internet-Dateien: {internetFilesSubDirectoryCreateResult.Error}");
                }
            }

            var integratedInternetFiles = new List<SupportedVideo>();
            foreach (var internetStreamingVideoFile in mediaSet.InternetStreamingVideoFiles.Value)
            {
                var internetStreamingVideoFileTargetPath = Path.Combine(internetFilesSubDirectory.FullName, internetStreamingVideoFile.FileInfo.Name);
                _logger.LogInformation("Verschiebe Internet-Datei: {internetStreamingVideoFileTargetPath}", internetStreamingVideoFileTargetPath);
                var internetStreamingVideoFileMoveResult = await _fileOperations.MoveFileAsync(internetStreamingVideoFile.FileInfo.FullName, internetStreamingVideoFileTargetPath, true);
                if (internetStreamingVideoFileMoveResult.IsFailure)
                {
                    return Result.Failure<List<SupportedVideo>>($"Fehler beim Verschieben der Internet-Datei: {internetStreamingVideoFileMoveResult.Error}");
                }

                // Aktualisiere den Dateipfad des Videos
                internetStreamingVideoFile.UpdateFilePath(internetStreamingVideoFileTargetPath);
            }

            return Result.Success(integratedInternetFiles);
        }
    }

    private async Task<Result> IntegrateMediaServerFiles(MediaSet mediaSet, DirectoryInfo mediaSetTargetDirectory)
    {
        _logger.LogInformation("Verschiebe Medien-Dateien in das Medienset-Verzeichnis: {mediaSetDirectory}", mediaSetTargetDirectory.FullName);
        if (mediaSet.LocalMediaServerVideoFile.HasNoValue)
        {
            _logger.LogInformation("Keine Medien-Dateien für den Medienserver vorhanden.");
            return Result.Success(Maybe<SupportedVideo>.None);
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
                    return Result.Failure<Maybe<SupportedVideo>>($"Fehler beim Erstellen des Unterverzeichnisses für Medienserver-Dateien: {mediaServerFilesSubDirectoryCreateResult.Error}");
                }
            }

            var videoFileForMediaServerMoveResult = await _fileOperations.MoveFileAsync(videoFileForMediaServer.FileInfo.FullName, videoFileForMediaServerTargetPath, true);
            if (videoFileForMediaServerMoveResult.IsFailure)
            {
                return Result.Failure<Maybe<SupportedVideo>>($"Fehler beim Verschieben der Video-Datei für Medienserver: {videoFileForMediaServerMoveResult.Error}");
            }
            _logger.LogInformation("Video-Datei für Medienserver erfolgreich verschoben.");

            // Aktualisiere den Dateipfad des Videos
            videoFileForMediaServer.UpdateFilePath(videoFileForMediaServerTargetPath);

            return Result.Success(Maybe<SupportedVideo>.From(videoFileForMediaServer));
        }
    }
}

public record MediaSetDirectory(List<MediaSet> IntegratedMediaSetDirectories);