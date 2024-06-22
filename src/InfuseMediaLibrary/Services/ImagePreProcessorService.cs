using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.Common.Services.ImageProcessing;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.Common.Models;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

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
    /// Wandelt den Farbraum einer Bilddatei von BT.2020 in Adobe RGB um. Die konvertierte Datei hat das Suffix "-adobe_rgb".
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task<Result<FileInfo>> ConvertColorSpaceAsyncToAdobeRGB(FileInfo filePath)
    {
        if (filePath == null)
        {
            return Result.Failure<FileInfo>("Die Bilddatei darf nicht null sein.");
        }

        if (!filePath.Exists)
        {
            return Result.Failure<FileInfo>($"Die Bilddatei {filePath.FullName} existiert nicht.");
        }

        // Prüfe die Einstellungen für das Suffix für konvertierte Dateien
        if (string.IsNullOrWhiteSpace(_moduleSettings.SuffixForConvertedTempImage))
        {
            return Result.Failure<FileInfo>("Das Suffx für konvertierte temporäre Dateien ist nicht konfiguriert.");
        }
        _logger.LogInformation($"Suffix für konvertierte temporäre Dateien: {_moduleSettings.SuffixForConvertedTempImage}");

        // Ermittle den Dateinamen für die konvertierte Datei mit dem Suffix, z.B. "-adobe_rgb"
        var directoryPath = filePath.Directory;
        if (directoryPath == null)
        {
            return Result.Failure<FileInfo>($"Fehler beim Ermitteln des Verzeichnisses für die Bilddatei {filePath.FullName}");
        }
        var convertedFilePath = Path.Combine(directoryPath.FullName, $"{Path.GetFileNameWithoutExtension(filePath.Name)}{_moduleSettings.SuffixForConvertedTempImage}{filePath.Extension}");
        _logger.LogInformation($"Dateiname für konvertierte Datei: {convertedFilePath}");

        var colorConversationResult = await _colorConversionService.ConvertColorSpaceAsync(filePath.FullName, convertedFilePath, "bt2020", "adobe_rgb");
        if (colorConversationResult.IsFailure)
        {
            return Result.Failure<FileInfo>($"Fehler beim Konvertieren des Farbraums von BT.2020 nach Adobe RGB: {colorConversationResult.Error}");
        }
        _logger.LogInformation($"Erfolgreiches Konvertieren des Farbraums von BT.2020 nach Adobe RGB: {filePath.FullName}");
        return new FileInfo(convertedFilePath);
    }
}