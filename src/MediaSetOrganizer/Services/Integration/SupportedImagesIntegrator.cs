using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Kurmann.Videoschnitt.MediaSetOrganizer.Services.Imaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.MediaSetOrganizer.Services.Integration;

public class SupportedImagesIntegrator
{
    private readonly ILogger<SupportedImagesIntegrator> _logger;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;
    private readonly IFileOperations _fileOperations;
    private readonly PortraitAndLandscapeService _portraitAndLandscapeService;

    public SupportedImagesIntegrator(ILogger<SupportedImagesIntegrator> logger, IOptions<MediaSetOrganizerSettings> mediaSetOrganizerSettings,
        IFileOperations fileOperations, PortraitAndLandscapeService portraitAndLandscapeService)
    {
        _logger = logger;
        _mediaSetOrganizerSettings = mediaSetOrganizerSettings.Value;
        _fileOperations = fileOperations;
        _portraitAndLandscapeService = portraitAndLandscapeService;
    }

    public async Task<Result> IntegrateImageFiles(MediaSet mediaSet, DirectoryInfo mediaSetTargetDirectory)
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

            foreach (var imageFile in mediaSet.ImageFiles.Value)
            {
                var updateImagePathResult = await _portraitAndLandscapeService.RenameImageFilesByAspectRatioAsync(mediaSet);
                if (updateImagePathResult.IsFailure)
                {
                    return Result.Failure<List<SupportedImage>>($"Fehler beim Aktualisieren des Dateipfads des Bildes: {updateImagePathResult.Error}");
                } 

                // Verschiebe die Original-Bilddatei
                var imageFileTargetPath = Path.Combine(imageFilesSubDirectory.FullName, imageFile.FileInfo.Name);
                _logger.LogInformation("Verschiebe Bild-Datei: {imageFileTargetPath}", imageFileTargetPath);
                var imageFileMoveResult = await _fileOperations.MoveFileAsync(imageFile.FileInfo.FullName, imageFileTargetPath, true);
                if (imageFileMoveResult.IsFailure)
                {
                    return Result.Failure<List<SupportedImage>>($"Fehler beim Verschieben der Bild-Datei: {imageFileMoveResult.Error}");
                }

                // Aktualisiere den Dateipfad des Bildes
                imageFile.UpdateFilePath(imageFileTargetPath);

                // Verschiebe die Adobe RGB-Bilddatei
                if (imageFile.FileInfoAdobeRgb.HasValue)
                {
                    var imageFileAdobeRgbTargetPath = Path.Combine(imageFilesSubDirectory.FullName, imageFile.FileInfoAdobeRgb.Value.Name);
                    _logger.LogInformation("Verschiebe Adobe RGB-Bild-Datei: {imageFileAdobeRgbTargetPath}", imageFileAdobeRgbTargetPath);
                    var imageFileAdobeRgbMoveResult = await _fileOperations.MoveFileAsync(imageFile.FileInfoAdobeRgb.Value.FullName, imageFileAdobeRgbTargetPath, true);
                    if (imageFileAdobeRgbMoveResult.IsFailure)
                    {
                        return Result.Failure<List<SupportedImage>>($"Fehler beim Verschieben der Adobe RGB-Bild-Datei: {imageFileAdobeRgbMoveResult.Error}");
                    }

                    // Aktualisiere den Dateipfad der Adobe RGB-Bilddatei
                    imageFile.UpdateFilePathAdobeRgb(imageFileAdobeRgbTargetPath);
                }      
                
            }

            return Result.Success();
        }
    }
}
