using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.Integration;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

/// <summary>
/// Verantwortlich für die Integration von Mediensets in die lokale Infuse-Mediathek.
/// </summary>
internal class MediaIntegratorService
{
    private readonly ILogger<MediaIntegratorService> _logger;
    private readonly IFileOperations _fileOperations;
    private readonly ApplicationSettings _applicationSettings; 
    private readonly ArtworkImageIntegrator _artworkImageIntegrator;
    private readonly VideoMetadataService _videoMetadataService;
    private readonly VideoIntegratorService _videoIntegratorService;
    private readonly TargetPathService _targetPathService;

    public MediaIntegratorService(ILogger<MediaIntegratorService> logger, IFileOperations fileOperations, IOptions<ApplicationSettings> applicationSettings, 
        ArtworkImageIntegrator artworkImageIntegrator, VideoMetadataService videoMetadataService, VideoIntegratorService videoIntegratorService,
        TargetPathService targetPathService)
    {
        _logger = logger;
        _fileOperations = fileOperations;
        _applicationSettings = applicationSettings.Value;
        _artworkImageIntegrator = artworkImageIntegrator;
        _videoMetadataService = videoMetadataService;
        _videoIntegratorService = videoIntegratorService;
        _targetPathService = targetPathService;
    }

    public async Task<Result> IntegrateMediaSetToLocalInfuseMediaLibrary(FileInfo videoFile, IEnumerable<FileInfo> imageFiles)
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

        var videoIntegrationResult = await _videoIntegratorService.IntegrateVideoAsync(videoFile);

        // Verschiebe die Video-Datei in das Infuse-Mediathek-Verzeichnis und erstelle das Verzeichnis falls es nicht existiert
        var targetDirectory = await _targetPathService.GetTargetDirectoryAsync(videoFile);
        if (targetDirectory.IsFailure)
        {
            return Result.Failure<Maybe<LocalMediaServerFiles>>($"Das Zielverzeichnis für die Integration in die Infuse-Mediathek konnte nicht ermittelt werden: {targetDirectory.Value.FullName}");
        }
        if (!targetDirectory.Value.Exists)
        {
            _logger.LogInformation("Das Zielverzeichnis für die Integration in die Infuse-Mediathek existiert nicht. Erstelle Verzeichnis: {targetDirectory.FullName}", targetDirectory.Value.FullName);
            var createDirectoryResult = await _fileOperations.CreateDirectoryAsync(targetDirectory.Value.FullName);
            if (createDirectoryResult.IsFailure)
            {
                return Result.Failure<Maybe<LocalMediaServerFiles>>($"Das Zielverzeichnis für die Integration in die Infuse-Mediathek konnte nicht erstellt werden: {targetDirectory.Value.FullName}. Fehler: {createDirectoryResult.Error}");
            }
        }
/* 
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
        var movedSupportedImagesResult = await _artworkImageIntegrator.IntegrateImages(imageFiles.ToList(), targetFilePathResult.Value);
        if (movedSupportedImagesResult.IsFailure)
        {
            _logger.LogWarning("Die Bild-Dateien konnten nicht in das Infuse-Mediathek-Verzeichnis {targetDirectory.FullName} verschoben werden: {movedSupportedImagesResult.Error}", targetDirectory.FullName, movedSupportedImagesResult.Error);
            _logger.LogInformation("Es werden keine Bild-Dateien in das Infuse-Mediathek-Verzeichnis verschoben.");
        } */

        return Result.Success();
    }
}
