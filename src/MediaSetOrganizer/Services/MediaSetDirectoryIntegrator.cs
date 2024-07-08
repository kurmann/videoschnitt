using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
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

    public async Task<Result<List<MediaSet>>> IntegrateInLocalMediaSetDirectory(IEnumerable<MediaSet> mediaSets)
    {
        _logger.LogInformation("Integriere Mediensets in lokales Medienset-Verzeichnis.");
        _logger.LogInformation("Berücksichtige den Einsatzzweck der Medien indem diese in ein vordefiniertes Unterverzeichnis verschoben werden.");
        _logger.LogInformation("Unterverzeichnis für Medienserver: {mediaServerFilesSubDirectoryName}", _mediaSetOrganizerSettings.MediaSet.MediaServerFilesSubDirectoryName);
        _logger.LogInformation("Unterverzeichnis für Internet: {internetFilesSubDirectoryName}", _mediaSetOrganizerSettings.MediaSet.InternetFilesSubDirectoryName);
        _logger.LogInformation("Unterverzeichnis für Titelbilder: {imageFilesSubDirectoryName}", _mediaSetOrganizerSettings.MediaSet.ImageFilesSubDirectoryName);
        _logger.LogInformation("Unterverzeichnis für Masterdatei: {masterfileSubDirectoryName}", _mediaSetOrganizerSettings.MediaSet.MasterfileSubDirectoryName);

        var integratedMediaSets = new List<MediaSet>();
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

            var inteagrateImageFilesResult = await IntegrateImageFiles(mediaSet, mediaSetTargetDirectory);
            if (inteagrateImageFilesResult.IsFailure)
            {
                return Result.Failure<List<MediaSet>>($"Fehler beim Integrieren der Bild-Dateien: {inteagrateImageFilesResult.Error}");
            }

            var integratedMasterfileResult = await IntegrateMasterfile(mediaSet, mediaSetTargetDirectory);
            if (integratedMasterfileResult.IsFailure)
            {
                return Result.Failure<List<MediaSet>>($"Fehler beim Integrieren der Masterdatei: {integratedMasterfileResult.Error}");
            }

            _logger.LogInformation("Medienset erfolgreich integriert: {mediaSetTitle}", mediaSet.Title);
            var integratedMediaSet = new MediaSet
            {
                Title = mediaSet.Title,
                LocalMediaServerVideoFile = integrateMediaServerFilesResult.Value,
                InternetStreamingVideoFiles = integrateInternetFilesResult.Value,
                ImageFiles = inteagrateImageFilesResult.Value,
                Masterfile = integratedMasterfileResult.Value
            };
            integratedMediaSets.Add(integratedMediaSet);
        }

        return Result.Success(integratedMediaSets);
    }

    private async Task<Result<Maybe<Masterfile>>> IntegrateMasterfile(MediaSet mediaSet, DirectoryInfo mediaSetTargetDirectory)
    {
        _logger.LogInformation("Verschiebe Masterdatei in das Medienset-Verzeichnis: {mediaSetDirectory}", mediaSetTargetDirectory.FullName);
        if (mediaSet.Masterfile.HasNoValue)
        {
            _logger.LogInformation("Keine Masterdatei für den Medienserver vorhanden.");
            return Maybe<Masterfile>.None;
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

            // Füge die Masterdatei mit dem neuen Pfad zur Liste der integrierten Masterdateien hinzu
            try
            {
                var newMasterFilePath = Path.Combine(masterfileSubDirectory.FullName, mediaSet.Masterfile.Value.FileInfo.Name);
                var newMasterFileInfo = new FileInfo(newMasterFilePath);

                var integratedMasterfile = new Masterfile(newMasterFileInfo, mediaSet.Masterfile.Value.Codec, mediaSet.Masterfile.Value.Profile);
                return Maybe<Masterfile>.From(integratedMasterfile);
            }
            catch (Exception ex)
            {
                return Result.Failure<Maybe<Masterfile>>($"Fehler beim Erstellen des neuen Masterdatei-Objekts: {ex.Message}");
            }
        }
    }

    private async Task<Result<List<SupportedImage>>> IntegrateImageFiles(MediaSet mediaSet, DirectoryInfo mediaSetTargetDirectory)
    {
        _logger.LogInformation("Verschiebe Bild-Dateien in das Medienset-Verzeichnis: {mediaSetDirectory}", mediaSetTargetDirectory.FullName);
        if (mediaSet.ImageFiles.HasNoValue)
        {
            _logger.LogInformation("Keine Bild-Dateien für den Medienserver vorhanden.");
            return Result.Success(new List<SupportedImage>());
        }
        else
        {
            var imageFilesSubDirectory = new DirectoryInfo(Path.Combine(mediaSetTargetDirectory.FullName, _mediaSetOrganizerSettings.MediaSet.ImageFilesSubDirectoryName));
            if (!imageFilesSubDirectory.Exists)
            {
                _logger.LogInformation("Erstelle Unterverzeichnis für Bild-Dateien: {imageFilesSubDirectory}", imageFilesSubDirectory.FullName);
                var imageFilesSubDirectoryCreateResult = await _fileOperations.CreateDirectoryAsync(imageFilesSubDirectory.FullName);
                if (imageFilesSubDirectoryCreateResult.IsFailure)
                {
                    return Result.Failure<List<SupportedImage>>($"Fehler beim Erstellen des Unterverzeichnisses für Bild-Dateien: {imageFilesSubDirectoryCreateResult.Error}");
                }
            }

            var integratedImages = new List<SupportedImage>();
            foreach (var imageFile in mediaSet.ImageFiles.Value)
            {
                var imageFileTargetPath = Path.Combine(imageFilesSubDirectory.FullName, imageFile.FileInfo.Name);
                _logger.LogInformation("Verschiebe Bild-Datei: {imageFileTargetPath}", imageFileTargetPath);
                var imageFileMoveResult = await _fileOperations.MoveFileAsync(imageFile.FileInfo.FullName, imageFileTargetPath, true);
                if (imageFileMoveResult.IsFailure)
                {
                    return Result.Failure<List<SupportedImage>>($"Fehler beim Verschieben der Bild-Datei: {imageFileMoveResult.Error}");
                }

                // Füge das Bild mit dem neuen Pfad zur Liste der integrierten Bilder hinzu
                var integratedImage = SupportedImage.Create(imageFilesSubDirectory.FullName, imageFile.FileInfo.Name);
                if (integratedImage.IsFailure)
                {
                    return Result.Failure<List<SupportedImage>>($"Fehler beim Erstellen des integrierten Bildes: {integratedImage.Error}");
                }
                integratedImages.Add(integratedImage.Value);
            }

            return integratedImages;
        }
    }

    private async Task<Result<List<SupportedVideo>>> IntegrateInternetFiles(MediaSet mediaSet, DirectoryInfo mediaSetTargetDirectory)
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

                // Füge das Video mit dem neuen Pfad zur Liste der integrierten Videos hinzu
                var integratedVideo = SupportedVideo.Create(internetFilesSubDirectory.FullName, internetStreamingVideoFile.FileInfo.Name);
                if (integratedVideo.IsFailure)
                {
                    return Result.Failure<List<SupportedVideo>>($"Fehler beim Erstellen des integrierten Videos: {integratedVideo.Error}");
                }
                integratedInternetFiles.Add(integratedVideo.Value);
            }

            return integratedInternetFiles;
        }
    }

    private async Task<Result<Maybe<SupportedVideo>>> IntegrateMediaServerFiles(MediaSet mediaSet, DirectoryInfo mediaSetTargetDirectory)
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

            // Füge das Video mit dem neuen Pfad zur Liste der integrierten Videos hinzu
            var integratedVideo = SupportedVideo.Create(mediaServerFilesSubDirectory.FullName, videoFileForMediaServer.FileInfo.Name);
            if (integratedVideo.IsFailure)
            {
                return Result.Failure<Maybe<SupportedVideo>>($"Fehler beim Erstellen des integrierten Videos: {integratedVideo.Error}");
            }

            return Maybe<SupportedVideo>.From(integratedVideo.Value);
        }
    }
}

public record MediaSetDirectory(List<MediaSet> IntegratedMediaSetDirectories);