using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Services.Metadata;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.MediaSetOrganizer.Services.Imaging;

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
    /// Benennt die Bilddateien eines Mediensets auf Basis des Seitenverhältnisses um.
    /// </summary>
    /// <param name="mediaSet"></param>
    public async Task<Result> RenameImageFilesByAspectRatioAsync(MediaSet mediaSet)
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
                return Result.Failure($"Fehler beim Ermitteln des Bildformats bei der Datei {mediaSet.SingleImage.FileInfo.Name}: {detectResult.Error}");
            }

            // Ermittle neuer Dateiname auf Basis des Seitenverhältnisses mit Schema <MediaSet-Title><Suffix><Dateiendung>
            var orientationSuffix = detectResult.Value == ImageOrientation.Portrait ?
                _mediaSetOrganizerSettings.MediaSet.OrientationSuffixes.Portrait :
                _mediaSetOrganizerSettings.MediaSet.OrientationSuffixes.Landscape;

            var newFileName = $"{mediaSet.Name}{orientationSuffix}{mediaSet.SingleImage.FileInfo.Extension}";
            if (mediaSet.SingleImage.FileInfoAdobeRgb.HasNoValue)
            {
                return Result.Failure($"Fehler beim Ermitteln des Dateipfades der Adobe RGB-Bilddatei {mediaSet.SingleImage.FileInfo.Name}.");
            }
            var newFileNameAdobeRgb = $"{mediaSet.Name}{orientationSuffix}{mediaSet.SingleImage.FileInfoAdobeRgb.Value.Extension}";

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
                File.Move(mediaSet.SingleImage.FileInfo.FullName, newFilePath, true);
                File.Move(mediaSet.SingleImage.FileInfoAdobeRgb.Value.FullName, newFilePathAdobeRgb, true);

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
            // Falls mehrere Bilder vorhanden sind, berücksichtigen wir für die Umbenennung nur die zwei zuletzt geschriebenen Bilder
            var images = mediaSet.GetImagesOrderedByLastWriteTime();
            var file0 = images.ElementAt(0);
            var file1 = images.ElementAt(1);

            // Lege fest, welches Bild im Hoch- und Querformat verwendet wird
            var detectResult = await DetectPortraitAndLandscapeImagesAsync(file0, file1);
            if (detectResult.IsFailure)
            {
                return Result.Failure($"Fehler beim Ermitteln des Bildformats: {detectResult.Error}");
            }

            // Benenne die Originalbilder und Adobe RGB-Bilder um anhand des Seitenverhältnisses
            var renameOriginalImagesResult = RenameImages(detectResult.Value.PortraitImage, detectResult.Value.LandscapeImage);
            if (renameOriginalImagesResult.IsFailure)
            {
                return Result.Failure($"Fehler beim Umbenennen der Original- und Adobe RGB-Bilddateien: {renameOriginalImagesResult.Error}");
            }
        }

        return Result.Success();

        Result RenameImages(SupportedImage portraitImage, SupportedImage landscapeImage)
        {
            // Prüfe ob beide Bilder vorhanden sind
            if (portraitImage == null || landscapeImage == null)
            {
                return Result.Failure("Es konnten keine Bilder für die Umbenennung gefunden werden.");
            }

            var newFileNameForPortrait = $"{mediaSet.Name}{_mediaSetOrganizerSettings.MediaSet.OrientationSuffixes.Portrait}{portraitImage.FileInfo.Extension}";
            var newFileNameForLandscape = $"{mediaSet.Name}{_mediaSetOrganizerSettings.MediaSet.OrientationSuffixes.Landscape}{landscapeImage.FileInfo.Extension}";
            var newFileNameForPortraitAdobeRgb = $"{mediaSet.Name}{_mediaSetOrganizerSettings.MediaSet.OrientationSuffixes.Portrait}{portraitImage.FileInfoAdobeRgb.Value.Extension}";
            var newFileNameForLandscapeAdobeRgb = $"{mediaSet.Name}{_mediaSetOrganizerSettings.MediaSet.OrientationSuffixes.Landscape}{landscapeImage.FileInfoAdobeRgb.Value.Extension}";

            // Nimm an, dass die beiden Bilder im gleichen Verzeichnis liegen
            var directoryName = portraitImage.FileInfo.DirectoryName;
            if (directoryName == null)
            {
                return Result.Failure($"Fehler beim Ermitteln des Verzeichnisses der Bilddatei {portraitImage.FileInfo.Name}.");
            }

            var newFilePathForPortrait = Path.Combine(directoryName, newFileNameForPortrait);
            var newFilePathForLandscape = Path.Combine(directoryName, newFileNameForLandscape);
            var newFilePathForPortraitAdobeRgb = Path.Combine(directoryName, newFileNameForPortraitAdobeRgb);
            var newFilePathForLandscapeAdobeRgb = Path.Combine(directoryName, newFileNameForLandscapeAdobeRgb);

            // Porträt-Bild umbenennen
            try
            {
                // Benne die Datei um und überschreibe ggf. vorhandene Dateien
                File.Move(portraitImage.FileInfo.FullName, newFilePathForPortrait, true);
                File.Move(portraitImage.FileInfoAdobeRgb.Value.FullName, newFilePathForPortraitAdobeRgb, true);

                // Aktualisiere die Dateinamen im Medienset
                portraitImage.UpdateFilePath(newFilePathForPortrait);
                portraitImage.UpdateFilePathAdobeRgb(newFilePathForPortraitAdobeRgb);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Fehler beim Umbenennen der Bilddatei {portraitImage.FileInfo.Name}: {ex.Message}");
            }

            // Landscape-Bild umbenennen
            try
            {
                // Benne die Datei um und überschreibe ggf. vorhandene Dateien
                File.Move(landscapeImage.FileInfo.FullName, newFilePathForLandscape, true);
                File.Move(landscapeImage.FileInfoAdobeRgb.Value.FullName, newFilePathForLandscapeAdobeRgb, true);

                // Aktualisiere die Dateinamen im Medienset
                landscapeImage.UpdateFilePath(newFilePathForLandscape);
                landscapeImage.UpdateFilePathAdobeRgb(newFilePathForLandscapeAdobeRgb);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Fehler beim Umbenennen der Bilddatei {landscapeImage.FileInfo.Name}: {ex.Message}");
            }

            return Result.Success();
        }

    }

    /// <summary>
    /// Ermittelt, ob ein Bild im Hoch- oder Querformat vorliegt.
    /// </summary>
    public async Task<Result<ImageOrientation>> DetectPortraitAndLandscapeImagesAsync(FileInfo image)
    {
        var dimensionsResult = await _sipMetadataService.GetImageDimensionsWithSipsAsync(image.FullName);
        if (dimensionsResult.IsFailure)
            return Result.Failure<ImageOrientation>(dimensionsResult.Error);

        var (width, height) = dimensionsResult.Value;
        var isPortrait = height > width;

        _logger.LogInformation("Bild {Filename} hat die Auflösung {Width}x{Height} und ist im Format {Format}.", image.Name, width, height, isPortrait ? "Portrait" : "Landscape");
        _logger.LogInformation("Das Bild wird somit als {Orientation} verwendet.", isPortrait ? "Portrait" : "Landscape");

        return isPortrait ? ImageOrientation.Portrait : ImageOrientation.Landscape;
    }

    /// <summary>
    /// Ermittelt die Bilddateien, die als Portrait und Landscape verwendet werden sollen. Geht vom Originalbild aus und nicht der Adobe RGB-Version.
    /// </summary>
    public async Task<Result<DetectPortraitAndLandscapeImagesResponse>> DetectPortraitAndLandscapeImagesAsync(SupportedImage firstImage, SupportedImage secondImage)
    {
        // Ermittle Bildformat der beiden Originalbilder
        var dimensionsResult1 = await _sipMetadataService.GetImageDimensionsWithSipsAsync(firstImage.FileInfo.FullName);
        var dimensionsResult2 = await _sipMetadataService.GetImageDimensionsWithSipsAsync(secondImage.FileInfo.FullName);

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

            _logger.LogInformation("Bild {Filename} hat die Auflösung {Width1}x{Height1} und ein Seitenverhältnis von {AspectRatio1}.", firstImage.FileInfo.Name, width1, height1, img1AspectRatio);
            _logger.LogInformation("Bild {Filename} hat die Auflösung {Width2}x{Height2} und ein Seitenverhältnis von {AspectRatio2}.", secondImage.FileInfo.Name, width2, height2, img2AspectRatio);

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

public enum ImageOrientation
{
    Portrait,
    Landscape
}

public record DetectPortraitAndLandscapeImagesResponse(SupportedImage PortraitImage, SupportedImage LandscapeImage);