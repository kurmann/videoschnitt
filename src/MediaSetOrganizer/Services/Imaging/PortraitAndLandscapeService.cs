using System.Drawing;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Imaging.Services;

/// <summary>
/// Verantwortlich für das Verwalten von Portrait- und Landscape-Bildern auf Basis von unterstützten Medien.
/// </summary>
public class PortraitAndLandscapeService
{
    /// <summary>
    /// Ermittelt, ob ein Bild im Hoch- oder Querformat vorliegt.
    /// </summary>
    public static Result<DetectPortraitAndLandscapeImagesResponse> DetectPortraitAndLandscapeImages(FileInfo image)
    {
        try
        {
            using var img = Image.FromFile(image.FullName);
            var isPortrait = img.Height > img.Width;
            return new DetectPortraitAndLandscapeImagesResponse(isPortrait ? image : null, isPortrait ? null : image);
        }
        catch (Exception ex)
        {
            return Result.Failure<DetectPortraitAndLandscapeImagesResponse>($"Fehler beim Laden des Bildes: {ex.Message}");
        }
    }

    /// <summary>
    /// Benennt alle Bilder pro Medienset um auf Basis des Seitenverhältnisses. Aktualisiert im Medienset die Dateinamen der Bilder.
    /// </summary>
    /// <param name="mediaSet"></param>
    public Task<Result> RenameImagesByAspectRatioAsync(IEnumerable<MediaSet> mediaSet)
    {
        // todo: wenn mehr als zwei Bilder pro Medienset vorhanden sind, nimm die beiden jüngsten Bilder und unterscheide bei diesen zwischen Portrait und Landscape
        return Task.FromResult(Result.Success());
    }

    /// <summary>
    /// Ermittelt die Bilddateien, die als Portrait und Landscape verwendet werden sollen.
    /// </summary>
    public static Result<DetectPortraitAndLandscapeImagesResponse> DetectPortraitAndLandscapeImages(FileInfo firstImage, FileInfo secondImage)
    {
        try
        {
            using var img1 = Image.FromFile(firstImage.FullName);
            using var img2 = Image.FromFile(secondImage.FullName);

            var img1IsPortrait = img1.Height > img1.Width;
            var img2IsPortrait = img2.Height > img2.Width;

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
                var img1AspectRatio = (double)img1.Width / img1.Height;
                var img2AspectRatio = (double)img2.Width / img2.Height;

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
        catch (Exception ex)
        {
            return Result.Failure<DetectPortraitAndLandscapeImagesResponse>($"Fehler beim Laden der Bilder: {ex.Message}");
        }
    }
}

public record DetectPortraitAndLandscapeImagesResponse(FileInfo? PortraitImage, FileInfo? LandscapeImage);