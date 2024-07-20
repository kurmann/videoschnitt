using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.Integration;

/// <summary>
/// Verantwortlich für das Verwalten von Postern und Fanarts auf Basis von unterstütuzten Medien.
/// </summary>
internal class PosterAndFanartService
{
    /// <summary>
    /// Ermittelt die Bilddateien, die als Poster und Fanart verwendet werden sollen.
    /// Die eine Bilddatei wird als Poster und die andere als Hintergrundbild verwendet.
    /// Die Festlegung erfolgt nach folgendem Schema nach Priorität:
    /// 1. Wenn eine Bilddatei im Dateinamen bereits "poster" enthält, wird diese als Poster verwendet und die andere als Hintergrundbild.
    ///    Und wenn eine Bilddatei im Dateinamen bereits "fanart" enthält, wird diese als Hintergrundbild verwendet und die andere als Poster.
    /// 2. Wenn beide Bilddateien das gleiche Seitenverhältnis haben, wird die jünge Bilddatei als Poster verwendet.
    /// Hinweis: Die Bildauflösungen werden über den FFMpegMetadataService ermittelt indem die Attribute "width" und "height" aus den Metadaten extrahiert werden.
    /// </summary>
    public static Result<DetectPosterAndFanartImagesResponse> DetectPosterAndFanartImages(SupportedImage firstImage, SupportedImage secondImage)
    {
        SupportedImage? posterImage;
        SupportedImage? fanartImage;

        // Priorität 1: Überprüfung auf spezifische Schlüsselwörter im Dateinamen
        if (firstImage.FullName.Contains("poster"))
        {
            posterImage = firstImage;
            fanartImage = secondImage;
        }
        else if (secondImage.Name.Contains("poster"))
        {
            posterImage = secondImage;
            fanartImage = firstImage;
        }
        else if (firstImage.Name.Contains("fanart"))
        {
            fanartImage = firstImage;
            posterImage = secondImage;
        }
        else if (secondImage.Name.Contains("fanart"))
        {
            fanartImage = secondImage;
            posterImage = firstImage;
        }

        // Priorität 3: Vergleich des Änderungsdatum, wenn Seitenverhältnisse gleich sind. Die jüngere Bilddatei wird als Poster verwendet.
        else
        {
            if (File.GetLastWriteTime(firstImage.FullName) > File.GetLastWriteTime(secondImage.FullName))
            {
                posterImage = firstImage;
                fanartImage = secondImage;
            }
            else
            {
                posterImage = secondImage;
                fanartImage = firstImage;
            }
        }

        if (posterImage == null || fanartImage == null)
            return Result.Failure<DetectPosterAndFanartImagesResponse>("Es konnten keine Poster- und Fanart-Bilddateien ermittelt werden.");

        return new DetectPosterAndFanartImagesResponse(posterImage, fanartImage);
    }
}

public record DetectPosterAndFanartImagesResponse(SupportedImage PosterImage, SupportedImage FanartImage);