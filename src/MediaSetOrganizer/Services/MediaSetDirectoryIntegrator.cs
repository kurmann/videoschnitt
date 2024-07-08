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
    private readonly ILogger<MediaSetDirectoryIntegrator> _logger;
    private readonly IFileOperations _fileOperations;

    public MediaSetDirectoryIntegrator(IOptions<ApplicationSettings> applicationSettings, ILogger<MediaSetDirectoryIntegrator> logger, IFileOperations fileOperations)
    {
        _applicationSettings = applicationSettings.Value;
        _logger = logger;
        _fileOperations = fileOperations;
    }

    public async Task<Result<List<DirectoryInfo>>> IntegrateInLocalMediaSetDirectory(List<MediaFilesByMediaSet> mediaFilesByMediaSets)
    {
        _logger.LogInformation("Integriere Mediensets in lokales Medienset-Verzeichnis.");
        var mediaSetDirectories = new List<DirectoryInfo>();

        foreach (var mediaFilesByMediaSet in mediaFilesByMediaSets)
        {
            if (mediaFilesByMediaSet.Title == null)
            {
                return Result.Failure<List<DirectoryInfo>>("Medienset-Titel ist null.");
            }

            var mediaSetTargetDirectory = new DirectoryInfo(Path.Combine(_applicationSettings.MediaSetPathLocal, mediaFilesByMediaSet.Title));
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
            var filesToMoveByMediaSet = mediaFilesByMediaSet.VideoFiles.Select(item => item.FileInfo)
                .Concat(mediaFilesByMediaSet.ImageFiles.Select(item => item.FileInfo))
                .ToList();
            foreach (var fileInfo in filesToMoveByMediaSet)
            {
                var destinationFile = new FileInfo(Path.Combine(mediaSetTargetDirectory.FullName, fileInfo.Name));
                var moveResult = await _fileOperations.MoveFileAsync(fileInfo.FullName, destinationFile.FullName, true);
                if (moveResult.IsFailure)
                {
                    return Result.Failure<List<DirectoryInfo>>($"Fehler beim Verschieben der Medien-Datei: {moveResult.Error}");
                }
                _logger.LogInformation("Medien-Datei verschoben: {destinationFile}", destinationFile.FullName);
            }

            mediaSetDirectories.Add(mediaSetTargetDirectory);
        }

        return Result.Success(mediaSetDirectories);
    }
}
