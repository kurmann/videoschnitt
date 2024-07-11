using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
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
    /// Benennt alle Bilder pro Medienset um auf Basis des Seitenverhältnisses. Aktualisiert im Medienset die Dateinamen der Bilder.
    /// </summary>
    /// <param name="mediaSet"></param>
    public async Task<Result> RenameImagesByAspectRatioAsync(IEnumerable<MediaSet> mediaSets)
    {
        // Iteriere durch alle Mediensets
        foreach (var mediaSet in mediaSets)
        {
            if (mediaSet.IsNoImageFile)
            {
                continue;
            }
            if (mediaSet.IsSingleImageFile)
            {
                var detectResult = await DetectPortraitAndLandscapeImagesAsync(mediaSet.GetSingleImage().FileInfo);
                if (detectResult.IsFailure)
                {
                    return Result.Failure($"Fehler beim Ermitteln des Bildformats: {detectResult.Error}");
                }

                // Ermittle neuer Dateiname auf Basis des Seitenverhältnisses mit Schema <MediaSet-Title><Suffix><Dateiendung>
                var orientationSuffix = detectResult.Value.PortraitImage != null ? 
                    _mediaSetOrganizerSettings.MediaSet.OrientationSuffixes.Portrait : 
                    _mediaSetOrganizerSettings.MediaSet.OrientationSuffixes.Landscape;

                var newFileName = $"{mediaSet.Title}{orientationSuffix}{mediaSet.GetSingleImage().FileInfo.Extension}";

                var directoryName = mediaSet.GetSingleImage().FileInfo.DirectoryName;
                if (directoryName == null)
                {
                    return Result.Failure($"Fehler beim Ermitteln des Verzeichnisses der Bilddatei {mediaSet.GetSingleImage().FileInfo.Name}.");
                }
                var newFilePath = Path.Combine(directoryName, newFileName);

                // Benenne Datei um
                try
                {
                    var supportedImage = mediaSet.GetSingleImage();
                    File.Move(supportedImage.FileInfo.FullName, newFilePath);

                    // Aktualisiere Dateiname im Medienset
                    var updatedSupportedImage = SupportedImage.CreateWithUpdatedFilePath(mediaSet.GetSingleImage(), newFilePath);
                    if (updatedSupportedImage.IsFailure)
                    {
                        return Result.Failure($"Fehler beim Aktualisieren des Dateipfads der Bilddatei {mediaSet.GetSingleImage().FileInfo.Name}: {updatedSupportedImage.Error}");
                    }
                    supportedImage = updatedSupportedImage.Value;
                }
                catch (Exception ex)
                {
                    return Result.Failure($"Fehler beim Umbenennen der Bilddatei {mediaSet.GetSingleImage().FileInfo.Name}: {ex.Message}");
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