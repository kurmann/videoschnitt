using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.Common.Services.ImageProcessing;
using Kurmann.Videoschnitt.ConfigurationModule.Services;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using System.Diagnostics;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;

namespace Kurmann.Videoschnitt.MediaSetOrganizer.Services.Imaging;

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
    /// Wandelt alle unterstützen Bilder nach JPG um und konvertiert den Farbraum von BT.2020 nach Adobe RGB.
    /// </summary>
    /// <param name="mediaSets"></param>
    /// <returns></returns>
    public async Task<Result<List<MediaSet>>> ConvertColorSpaceAndFormatAsync(IEnumerable<MediaSet> mediaSets)
    {
        foreach (var mediaSet in mediaSets)
        {
            var supportedImages = mediaSet.ImageFiles;
            if (supportedImages.HasNoValue || supportedImages.Value.Count == 0)
            {
                continue;
            }

            var processedImages = new List<SupportedImage>();
            foreach (var image in supportedImages.Value)
            {
                var convertedImageResult = await ConvertColorSpaceAndFormatAsync(image.FileInfo);
                if (convertedImageResult.IsFailure)
                {
                    _logger.LogError("Fehler beim Konvertieren des Farbraums und Formats der Bilddatei {image.FullName}: {convertedImageResult.Error}", image.FileInfo.FullName, convertedImageResult.Error);
                }
                else
                {
                    _logger.LogInformation("Erfolgreiches Konvertieren des Farbraums und Formats der Bilddatei {image.FullName}", image.FileInfo.FullName);

                    // Wenn die konvertierte Datei nicht gleich der Originaldatei ist, füge sie zur Medienset hinzu
                    if (!convertedImageResult.Value.IsConvertedImageEqualToOriginalImage)
                    {
                        var supportedImageFileResult = SupportedImage.Create(convertedImageResult.Value.ConvertedImage, true);
                        if (supportedImageFileResult.IsFailure)
                        {
                            return Result.Failure<List<MediaSet>>($"Fehler beim Erstellen des SupportedImage-Objekts für die konvertierte Bilddatei {convertedImageResult.Value.ConvertedImage.FullName}: {supportedImageFileResult.Error}");
                        }
                        else
                        {
                            processedImages.Add(supportedImageFileResult.Value);
                        }
                    }
                    else
                    {
                        // aktualisiere die Originaldatei mit der konvertierten Datei und setze das Flag für Adobe RGB auf true
                        var supportedImageAdobeRgb = SupportedImage.Create(convertedImageResult.Value.ConvertedImage, true);
                        if (supportedImageAdobeRgb.IsFailure)
                        {
                            return Result.Failure<List<MediaSet>>($"Fehler beim Erstellen des SupportedImage-Objekts für die konvertierte Bilddatei {convertedImageResult.Value.ConvertedImage.FullName}: {supportedImageAdobeRgb.Error}");
                        }
                        else
                        {
                            processedImages.Add(supportedImageAdobeRgb.Value);
                        }
                    }
                }
            }

            // Erstetze die Bild-Dateien im Medienset mit den konvertierten Bildern
            if (processedImages.Count > 0)
            {
                mediaSet.ImageFiles = processedImages;
            }

            // Füge die TIFF- oder PNG-Dateien, die nicht konvertiert werden konnten, zur Medienset hinzu damit sie nicht verloren gehen
            var unprocessedImages = supportedImages.Value.Where(i => !processedImages.Any(p => p.FileInfo.FullName == i.FileInfo.FullName));
            foreach (var unprocessedImage in unprocessedImages)
            {
                processedImages.Add(unprocessedImage);
            }
        }

        return Result.Success(mediaSets.ToList());
    }

    /// <summary>
    /// Wandelt den Farbraum einer Bilddatei von BT.2020 in Adobe RGB um und konvertiert sie bei Bedarf nach JPEG. Die konvertierte Datei hat das Suffix "-adobe_rgb".
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task<Result<ConvertColorSpaceAndFormatResponse>> ConvertColorSpaceAndFormatAsync(FileInfo filePath)
    {
        if (filePath == null)
        {
            return Result.Failure<ConvertColorSpaceAndFormatResponse>("Die Bilddatei darf nicht null sein.");
        }

        if (!filePath.Exists)
        {
            return Result.Failure<ConvertColorSpaceAndFormatResponse>($"Die Bilddatei {filePath.FullName} existiert nicht.");
        }

        var isTiffOrPngSource = filePath.Extension.Equals(".tiff", StringComparison.OrdinalIgnoreCase) ||
            filePath.Extension.Equals(".tif", StringComparison.OrdinalIgnoreCase) ||
            filePath.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase);


        // Konvertiere TIFF oder PNG Dateien nach JPEG
        Maybe<FileInfo> convertedJpgImageFromTiffOrPng = Maybe<FileInfo>.None;
        if (isTiffOrPngSource)
        {
            var jpegConversionResult = ConvertToJpegUsingSips(filePath);
            if (jpegConversionResult.IsFailure)
            {
                return Result.Failure<ConvertColorSpaceAndFormatResponse>($"Fehler beim Konvertieren der Datei {filePath.FullName} nach JPEG: {jpegConversionResult.Error}");
            }
            _logger.LogInformation("Erfolgreiches Konvertieren der Datei {filePath.FullName} nach JPEG: {convertedJpgImageFromTiffOrPng}", filePath.FullName, jpegConversionResult.Value);
            convertedJpgImageFromTiffOrPng = new FileInfo(jpegConversionResult.Value);
        }

        // Führe die Farbraumkonvertierung durch (überschreibe vorhande Datei)
        var sourceFileForColorConversion = convertedJpgImageFromTiffOrPng.HasValue ? convertedJpgImageFromTiffOrPng.Value : filePath;
        var colorConversionResult = await _colorConversionService.ConvertColorSpaceAsync(sourceFileForColorConversion.FullName, sourceFileForColorConversion.FullName, "bt2020", "adobe_rgb");
        if (colorConversionResult.IsFailure)
        {
            return Result.Failure<ConvertColorSpaceAndFormatResponse>($"Fehler beim Konvertieren des Farbraums von BT.2020 nach Adobe RGB: {colorConversionResult.Error}");
        }
        _logger.LogInformation("Erfolgreiches Konvertieren des Farbraums von BT.2020 nach Adobe RGB: {filePath.FullName}", filePath.FullName);

        return new ConvertColorSpaceAndFormatResponse(filePath, sourceFileForColorConversion);
    }

    public record ConvertColorSpaceAndFormatResponse(FileInfo OriginalImage, FileInfo ConvertedImage)
    {
        public bool IsConvertedImageEqualToOriginalImage => OriginalImage.Name == ConvertedImage.Name;
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