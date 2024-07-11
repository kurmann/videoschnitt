using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Services.Metadata;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Imaging.Services;

/// <summary>
/// Verantwortlich für das Verwalten von Portrait- und Landscape-Bildern auf Basis von unterstützten Medien.
/// </summary>
public class PortraitAndLandscapeService
{
    private readonly SipsMetadataService _sipMetadataService;
    private readonly ILogger<PortraitAndLandscapeService> _logger;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;

    public PortraitAndLandscapeService(SipsMetadataService sipMetadataService, ILogger<PortraitAndLandscapeService> logger, IOptions<MediaSetOrganizerSettings> mediaSetOrganizerSettings)
    {
        _sipMetadataService = sipMetadataService;
        _logger = logger;
        _mediaSetOrganizerSettings = mediaSetOrganizerSettings.Value;
    }

    /// <summary>
    /// Aktualisiert den oder die Dateipfade eines Mediensets auf Basis des Seitenverhältnisses der Bilder.
    /// </summary>
    /// <param name="mediaSet"></param>
    public async Task<Result> UpdateFilePathByAspectRatioAsync(MediaSet mediaSet)
    {
        if (mediaSet.IsNoImageFile)
        {
            return Result.Success();
        }
        if (mediaSet.IsSingleImageFile)
        {
            var detectResult = await DetectPortraitAndLandscapeImagesAsync(mediaSet.SingleImage.FileInfo);
            if (detectResult.IsFailure)
            {
                return Result.Failure($"Fehler beim Ermitteln des Bildformats: {detectResult.Error}");
            }

            // Ermittle neuer Dateiname auf Basis des Seitenverhältnisses mit Schema <MediaSet-Title><Suffix><Dateiendung>
            var orientationSuffix = detectResult.Value.PortraitImage != null ?
                _mediaSetOrganizerSettings.MediaSet.OrientationSuffixes.Portrait :
                _mediaSetOrganizerSettings.MediaSet.OrientationSuffixes.Landscape;

            var newFileName = $"{mediaSet.Title}{orientationSuffix}{mediaSet.SingleImage.FileInfo.Extension}";
            if (mediaSet.SingleImage.FileInfoAdobeRgb.HasNoValue)
            {
                return Result.Failure($"Fehler beim Ermitteln des Dateipfades der Adobe RGB-Bilddatei {mediaSet.SingleImage.FileInfo.Name}.");
            }
            var newFileNameAdobeRgb = $"{mediaSet.Title}{orientationSuffix}{mediaSet.SingleImage.FileInfoAdobeRgb.Value.Extension}";

            var directoryName = mediaSet.SingleImage.FileInfo.DirectoryName;
            if (directoryName == null)
            {
                return Result.Failure($"Fehler beim Ermitteln des Verzeichnisses der Bilddatei {mediaSet.SingleImage.FileInfo.Name}.");
            }
            var newFilePath = Path.Combine(directoryName, newFileName);
            var newFilePathAdobeRgb = Path.Combine(directoryName, newFileNameAdobeRgb);

            // Benne Dateien um
            try
            {
                File.Move(mediaSet.SingleImage.FileInfo.FullName, newFilePath);
                File.Move(mediaSet.SingleImage.FileInfoAdobeRgb.Value.FullName, newFilePathAdobeRgb);
                
                // Aktualisiere Dateinamen im Medienset
                mediaSet.SingleImage.UpdateFilePath(newFilePath);
                mediaSet.SingleImage.UpdateFilePathAdobeRgb(newFilePathAdobeRgb);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Fehler beim Umbenennen der Bilddatei {mediaSet.SingleImage.FileInfo.Name}: {ex.Message}");
            }
        }
        if (mediaSet.IsMultipleImageFiles)
        {
            var images = mediaSet.GetImagesOrderedByLastWriteTime();
            var detectResult = await DetectPortraitAndLandscapeImagesAsync(images.ElementAt(0).FileInfo, images.ElementAt(1).FileInfo);
            if (detectResult.IsFailure)
            {
                return Result.Failure($"Fehler beim Ermitteln des Bildformats: {detectResult.Error}");
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// Ermittelt, ob ein Bild im Hoch- oder Querformat vorliegt.
    /// </summary>
    public async Task<Result<DetectPortraitAndLandscapeImagesResponse>> DetectPortraitAndLandscapeImagesAsync(FileInfo image)
    {
        var dimensionsResult = await _sipMetadataService.GetImageDimensionsWithSipsAsync(image.FullName);
        if (dimensionsResult.IsFailure)
            return Result.Failure<DetectPortraitAndLandscapeImagesResponse>(dimensionsResult.Error);

        var (width, height) = dimensionsResult.Value;
        var isPortrait = height > width;

        _logger.LogInformation("Bild {Filename} hat die Auflösung {Width}x{Height} und ist im Format {Format}.", image.Name, width, height, isPortrait ? "Portrait" : "Landscape");
        _logger.LogInformation("Das Bild wird somit als {Orientation} verwendet.", isPortrait ? "Portrait" : "Landscape");

        return new DetectPortraitAndLandscapeImagesResponse(isPortrait ? image : null, isPortrait ? null : image);
    }

    /// <summary>
    /// Ermittelt die Bilddateien, die als Portrait und Landscape verwendet werden sollen.
    /// </summary>
    public async Task<Result<DetectPortraitAndLandscapeImagesResponse>> DetectPortraitAndLandscapeImagesAsync(FileInfo firstImage, FileInfo secondImage)
    {
        var dimensionsResult1 = await _sipMetadataService.GetImageDimensionsWithSipsAsync(firstImage.FullName);
        var dimensionsResult2 = await _sipMetadataService.GetImageDimensionsWithSipsAsync(secondImage.FullName);

        if (dimensionsResult1.IsFailure)
            return Result.Failure<DetectPortraitAndLandscapeImagesResponse>(dimensionsResult1.Error);
        if (dimensionsResult2.IsFailure)
            return Result.Failure<DetectPortraitAndLandscapeImagesResponse>(dimensionsResult2.Error);

        var (width1, height1) = dimensionsResult1.Value;
        var (width2, height2) = dimensionsResult2.Value;

        var img1IsPortrait = height1 > width1;
        var img2IsPortrait = height2 > width2;

        if (img1IsPortrait && !img2IsPortrait)
        {
            return new DetectPortraitAndLandscapeImagesResponse(firstImage, secondImage);
        }
        else if (!img1IsPortrait && img2IsPortrait)
        {
            return new DetectPortraitAndLandscapeImagesResponse(secondImage, firstImage);
        }
        else
        {
            // Beide Bilder haben das gleiche Format, jetzt nach Seitenverhältnis entscheiden
            var img1AspectRatio = (double)width1 / height1;
            var img2AspectRatio = (double)width2 / height2;

            _logger.LogInformation("Bild {Filename} hat die Auflösung {Width1}x{Height1} und ein Seitenverhältnis von {AspectRatio1}.", firstImage.Name, width1, height1, img1AspectRatio);
            _logger.LogInformation("Bild {Filename} hat die Auflösung {Width2}x{Height2} und ein Seitenverhältnis von {AspectRatio2}.", secondImage.Name, width2, height2, img2AspectRatio);

            if (img1AspectRatio > img2AspectRatio)
            {
                return new DetectPortraitAndLandscapeImagesResponse(secondImage, firstImage);
            }
            else
            {
                return new DetectPortraitAndLandscapeImagesResponse(firstImage, secondImage);
            }
        }
    }
}

public record DetectPortraitAndLandscapeImagesResponse(FileInfo? PortraitImage, FileInfo? LandscapeImage);