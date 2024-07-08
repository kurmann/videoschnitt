using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.Common.Services.ImageProcessing;
using Kurmann.Videoschnitt.ConfigurationModule.Services;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using System.Diagnostics;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

/// <summary>
/// Verantwortlich für die Vorverarbeitung von Bildern
/// </summary>
public class ImageProcessorService
{
    private readonly IColorConversionService _colorConversionService;
    private readonly ILogger<ImageProcessorService> _logger;
    private readonly InfuseMediaLibrarySettings _settings;

    public ImageProcessorService(ILogger<ImageProcessorService> logger, IColorConversionService colorConversionService, IConfigurationService configurationService)
    {
        _logger = logger;
        _colorConversionService = colorConversionService;
        _settings = configurationService.GetSettings<InfuseMediaLibrarySettings>();
    }

    /// <summary>
    /// Wandelt den Farbraum einer Bilddatei von BT.2020 in Adobe RGB um und konvertiert sie bei Bedarf nach JPEG. Die konvertierte Datei hat das Suffix "-adobe_rgb".
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task<Result<FileInfo>> ConvertColorSpaceAndFormatAsync(FileInfo filePath)
    {
        if (filePath == null)
        {
            return Result.Failure<FileInfo>("Die Bilddatei darf nicht null sein.");
        }

        if (!filePath.Exists)
        {
            return Result.Failure<FileInfo>($"Die Bilddatei {filePath.FullName} existiert nicht.");
        }

        // Konvertiere TIFF oder PNG Dateien nach JPEG
        if (filePath.Extension.Equals(".tiff", StringComparison.OrdinalIgnoreCase) ||
            filePath.Extension.Equals(".tif", StringComparison.OrdinalIgnoreCase) ||
            filePath.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
        {
            var jpegConversionResult = ConvertToJpegUsingSips(filePath);
            if (jpegConversionResult.IsFailure)
            {
                return Result.Failure<FileInfo>($"Fehler beim Konvertieren der Datei {filePath.FullName} nach JPEG: {jpegConversionResult.Error}");
            }
            filePath = new FileInfo(jpegConversionResult.Value);
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
        _logger.LogInformation("Dateiname für konvertierte Datei: {convertedFilePath}", convertedFilePath);

        // Führe die Farbraumkonvertierung durch
        var colorConversionResult = await _colorConversionService.ConvertColorSpaceAsync(filePath.FullName, convertedFilePath, "bt2020", "adobe_rgb");
        if (colorConversionResult.IsFailure)
        {
            return Result.Failure<FileInfo>($"Fehler beim Konvertieren des Farbraums von BT.2020 nach Adobe RGB: {colorConversionResult.Error}");
        }
        _logger.LogInformation("Erfolgreiches Konvertieren des Farbraums von BT.2020 nach Adobe RGB: {filePath.FullName}", filePath.FullName);
        return new FileInfo(convertedFilePath);
    }

    private Result<string> ConvertToJpegUsingSips(FileInfo filePath)
    {
        try
        {
            var directoryName = filePath.DirectoryName;
            if (string.IsNullOrWhiteSpace(directoryName))
            {
                return Result.Failure<string>("Der Dateipfad der zu konvertierenden Datei konnte nicht ermittelt werden.");
            }
            var jpegFilePath = Path.Combine(directoryName, Path.GetFileNameWithoutExtension(filePath.Name) + _settings.PreferredJpgExtension);

            var psi = new ProcessStartInfo
            {
                FileName = "sips",
                Arguments = $"-s format jpeg \"{filePath.FullName}\" --out \"{jpegFilePath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);

            if (process == null)
            {
                return Result.Failure<string>("Fehler beim Starten des SIPS-Prozesses.");
            }

            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                return Result.Success(jpegFilePath);
            }
            else
            {
                return Result.Failure<string>($"SIPS Konvertierung fehlgeschlagen mit Exit Code {process.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Fehler beim Konvertieren der Datei {filePath.FullName} nach JPEG mit SIPS: {ex.Message}", filePath.FullName, ex.Message);
            return Result.Failure<string>(ex.Message);
        }
    }

    public Result<string> GetFileNameWithConvertedSuffix(FileInfo filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath.Name))
        {
            return Result.Failure<string>("Der Dateipfad darf nicht null sein.");
        }

        if (_settings.SuffixForConvertedTempImage == null)
        {
            return Result.Failure<string>("Das Suffix für konvertierte temporäre Dateien ist nicht konfiguriert.");
        }

        return $"{Path.GetFileNameWithoutExtension(filePath.Name)}{_settings.SuffixForConvertedTempImage}{Path.GetExtension(filePath.Name)}";
    }

    public Result<string> GetFileNameWithoutConvertedSuffix(FileInfo filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath.Name))
        {
            return Result.Failure<string>("Der Dateipfad darf nicht null sein.");
        }

        if (_settings.SuffixForConvertedTempImage == null)
        {
            return Result.Failure<string>("Das Suffix für konvertierte temporäre Dateien ist nicht konfiguriert.");
        }

        return $"{Path.GetFileNameWithoutExtension(filePath.Name)}{filePath.Extension}";
    }
}