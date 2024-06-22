using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.Common.Services.ImageProcessing;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.Common.Models;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

/// <summary>
/// Verantwortlich für die Vorverarbeitung von Bildern
/// </summary>
public class ImagePreProcessorService
{
    private readonly IColorConversionService _colorConversionService;
    private readonly ILogger<ImagePreProcessorService> _logger;
    private readonly IFileOperations _fileOperations;
    private readonly ModuleSettings _moduleSettings;

    public ImagePreProcessorService(ILogger<ImagePreProcessorService> logger, IColorConversionService colorConversionService, IFileOperations fileOperations, IOptions<ModuleSettings> moduleSettings)
    {
        _logger = logger;
        _colorConversionService = colorConversionService;
        _fileOperations = fileOperations;
        _moduleSettings = moduleSettings.Value;
    }

    /// <summary>
    /// Wandelt den Farbraum einer Bilddatei von BT.2020 in Adobe RGB um. Verschiebt die Originaldatei in ein Unterverzeichnis als Backup und spätetere Referenz.
    /// Die konvertierte Datei wird im selben Verzeichnis gespeichert;
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task<Result> ConvertColorSpaceAsyncToAdobeRGB(FileInfo filePath, string subDirectoryNameForConvertedFiles)
    {
        if (filePath == null)
        {
            return Result.Failure("Die Bilddatei darf nicht null sein.");
        }

        if (!filePath.Exists)
        {
            return Result.Failure($"Die Bilddatei {filePath.FullName} existiert nicht.");
        }

        var directoryPath = filePath.Directory;
        if (directoryPath == null)
        {
            return Result.Failure($"Fehler beim Ermitteln des Verzeichnisses von {filePath.FullName}");
        }

        // Die konvertierte Datei soll einen temporären Namen erhalten, um die Originaldatei nicht zu überschreiben
        var temporaryFilePath = Path.Combine(directoryPath.FullName, $"{Guid.NewGuid()}{filePath.Extension}");

        var colorConversationResult = await _colorConversionService.ConvertColorSpaceAsync(filePath.FullName, temporaryFilePath, "bt2020", "adobe_rgb");
        if (colorConversationResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Konvertieren des Farbraums von BT.2020 nach Adobe RGB: {colorConversationResult.Error}");
        }
        _logger.LogInformation($"Erfolgreiches Konvertieren des Farbraums von BT.2020 nach Adobe RGB: {filePath.FullName}");


        try
        {
            var processedImagesDirectoryPath = Path.Combine(directoryPath.FullName, subDirectoryNameForConvertedFiles);
            var directoryCreatedAsync = await _fileOperations.CreateDirectoryAsync(processedImagesDirectoryPath);
            if (directoryCreatedAsync.IsFailure)
            {
                return Result.Failure($"Fehler beim Erstellen des Unterverzeichnisses '{subDirectoryNameForConvertedFiles}' für konvertierte Dateien: {directoryCreatedAsync.Error}");
            }

            var newFilePath = Path.Combine(processedImagesDirectoryPath, filePath.Name);
            _logger.LogInformation($"Verschiebe Originaldatei {filePath.FullName} nach {newFilePath}");
            var moveFileResult = await _fileOperations.MoveFileAsync(filePath.FullName, newFilePath);
            if (moveFileResult.IsFailure)
            {
                return Result.Failure($"Fehler beim Verschieben der Originaldatei {filePath.FullName} nach {newFilePath}: {moveFileResult.Error}");
            }

            _logger.LogInformation($"Benenne temporäre Datei {temporaryFilePath} in {filePath.FullName} um");
            var renameFileResult = await _fileOperations.MoveFileAsync(temporaryFilePath, filePath.FullName);
            if (renameFileResult.IsFailure)
            {
                return Result.Failure($"Fehler beim Umbenennen der temporären Datei {temporaryFilePath} in {filePath.FullName}: {renameFileResult.Error}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Fehler beim Erstellen des Unterverzeichnisses '{subDirectoryNameForConvertedFiles}' für konvertierte Dateien: {ex.Message}");
        }
    }
}