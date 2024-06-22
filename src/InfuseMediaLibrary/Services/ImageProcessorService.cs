using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.Common.Services.ImageProcessing;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

/// <summary>
/// Verantwortlich für die Vorverarbeitung von Bildern
/// </summary>
public class ImageProcessorService
{
    private readonly IColorConversionService _colorConversionService;
    private readonly ILogger<ImageProcessorService> _logger;
    private readonly IFileOperations _fileOperations;
    private readonly ModuleSettings _moduleSettings;

    public ImageProcessorService(ILogger<ImageProcessorService> logger, IColorConversionService colorConversionService, IFileOperations fileOperations, IOptions<ModuleSettings> moduleSettings)
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

        // Ermittle den Dateinamen für die konvertierte Datei mit dem Suffix, z.B. "-adobe_rgb"
        var directoryPath = filePath.Directory;
        if (directoryPath == null)
        {
            return Result.Failure<FileInfo>($"Fehler beim Ermitteln des Verzeichnisses für die Bilddatei {filePath.FullName}");
        }
        var convertedFileNameResult = GetFileNameWithConvertedSuffix(filePath);
        if (convertedFileNameResult.IsFailure)
        {
            return Result.Failure<FileInfo>($"Fehler beim Ermitteln des Dateinamens für die konvertierte Datei: {convertedFileNameResult.Error}");
        }

        var convertedFilePath = Path.Combine(directoryPath.FullName, convertedFileNameResult.Value);
        _logger.LogInformation($"Dateiname für konvertierte Datei: {convertedFilePath}");

        var colorConversationResult = await _colorConversionService.ConvertColorSpaceAsync(filePath.FullName, convertedFilePath, "bt2020", "adobe_rgb");
        if (colorConversationResult.IsFailure)
        {
            return Result.Failure<FileInfo>($"Fehler beim Konvertieren des Farbraums von BT.2020 nach Adobe RGB: {colorConversationResult.Error}");
        }
        _logger.LogInformation($"Erfolgreiches Konvertieren des Farbraums von BT.2020 nach Adobe RGB: {filePath.FullName}");
        return new FileInfo(convertedFilePath);
    }

    public Result<string> GetFileNameWithConvertedSuffix(FileInfo filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath.Name))
        {
            return Result.Failure<string>("Der Dateipfad darf nicht null sein.");
        }

        if (_moduleSettings.SuffixForConvertedTempImage == null)
        {
            return Result.Failure<string>("Das Suffx für konvertierte temporäre Dateien ist nicht konfiguriert.");
        }

        return $"{Path.GetFileNameWithoutExtension(filePath.Name)}{_moduleSettings.SuffixForConvertedTempImage}{filePath.Extension}";
    }

    public Result<string> GetFileNameWithoutConvertedSuffix(FileInfo filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath.Name))
        {
            return Result.Failure<string>("Der Dateipfad darf nicht null sein.");
        }

        if (_moduleSettings.SuffixForConvertedTempImage == null)
        {
            return Result.Failure<string>("Das Suffx für konvertierte temporäre Dateien ist nicht konfiguriert.");
        }

        return $"{Path.GetFileNameWithoutExtension(filePath.Name)}{filePath.Extension}";
    }
}