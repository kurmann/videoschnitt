using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Services;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;


namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

/// <summary>
/// Verantwortlich für das Verwalten von Postern und Fanarts auf Basis von unterstütuzten Medien.
/// </summary>
public class PosterAndFanartService
{
    private readonly ILogger<PosterAndFanartService> _logger;
    private readonly FFmpegMetadataService _ffmpegMetadataService;

    public PosterAndFanartService(ILogger<PosterAndFanartService> logger, FFmpegMetadataService ffmpegMetadataService)
    {
        _logger = logger;
        _ffmpegMetadataService = ffmpegMetadataService;
    }

    /// <summary>
    /// Ermittelt die Bilddateien, die als Poster und Fanart verwendet werden sollen.
    /// Die eine Bilddatei wird als Poster und die andere als Hintergrundbild verwendet.
    /// Die Festlegung erfolgt nach folgendem Schema nach Priorität:
    /// 1. Wenn eine Bilddatei im Dateinamen bereits "poster" enthält, wird diese als Poster verwendet und die andere als Hintergrundbild.
    ///    Und wenn eine Bilddatei im Dateinamen bereits "fanart" enthält, wird diese als Hintergrundbild verwendet und die andere als Poster.
    /// 2. Die Bilddatei mit dem breiteren Seitenverhältnis wird als Poster verwendet (bspw. 16:9 gegenüber 1:1).
    /// 3. Wenn beide Bilddateien das gleiche Seitenverhältnis haben, wird die jünge Bilddatei als Poster verwendet.
    /// Hinweis: Die Bildauflösungen werden über den FFMpegMetadataService ermittelt indem die Attribute "width" und "height" aus den Metadaten extrahiert werden.
    /// </summary>
    public async Task<Result<DetectPosterAndFanartImagesResponse>> DetectPosterAndFanartImages(SupportedImage firstImage, SupportedImage secondImage, DirectoryInfo targetDirectory)
    {
        SupportedImage? posterImage = null;
        SupportedImage? fanartImage = null;

        // Priorität 1: Überprüfung auf spezifische Schlüsselwörter im Dateinamen
        if (firstImage.FileInfo.FullName.Contains("poster"))
        {
            posterImage = firstImage;
            fanartImage = secondImage;
        }
        else if (secondImage.FileInfo.Name.Contains("poster"))
        {
            posterImage = secondImage;
            fanartImage = firstImage;
        }
        else if (firstImage.FileInfo.Name.Contains("fanart"))
        {
            fanartImage = firstImage;
            posterImage = secondImage;
        }
        else if (secondImage.FileInfo.Name.Contains("fanart"))
        {
            fanartImage = secondImage;
            posterImage = firstImage;
        }

        // Priorität 2: Vergleich der Seitenverhältnisse, wenn noch keine Zuordnung erfolgt ist
        if (posterImage == null || fanartImage == null)
        {
            var firstAspectRatio = await _ffmpegMetadataService.GetAspectRatioAsync(firstImage.FileInfo.FullName);
            if (firstAspectRatio.IsFailure)
                return Result.Failure<DetectPosterAndFanartImagesResponse>($"Das Seitenverhältnis der Bilddatei {firstImage.FileInfo.FullName} konnte nicht ermittelt werden: {firstAspectRatio.Error}");
            if (firstAspectRatio.Value == 0)
                return Result.Failure<DetectPosterAndFanartImagesResponse>($"Das Seitenverhältnis der Bilddatei {firstImage.FileInfo.FullName} ist 0.");
            
            var secondAspectRatio = await _ffmpegMetadataService.GetAspectRatioAsync(secondImage.FileInfo.FullName);
            if (secondAspectRatio.IsFailure)
                return Result.Failure<DetectPosterAndFanartImagesResponse>($"Das Seitenverhältnis der Bilddatei {secondImage.FileInfo.FullName} konnte nicht ermittelt werden: {secondAspectRatio.Error}");
            if (secondAspectRatio.Value == 0)
                return Result.Failure<DetectPosterAndFanartImagesResponse>($"Das Seitenverhältnis der Bilddatei {secondImage.FileInfo.FullName} ist 0.");

            if (firstAspectRatio.Value > secondAspectRatio.Value)
            {
                posterImage = firstImage;
                fanartImage = secondImage;
            }
            else if (secondAspectRatio.Value > firstAspectRatio.Value)
            {
                posterImage = secondImage;
                fanartImage = firstImage;
            }

            // Priorität 3: Vergleich des Änderungsdatum, wenn Seitenverhältnisse gleich sind. Die jüngere Bilddatei wird als Poster verwendet.
            else
            {
                if (File.GetLastWriteTime(firstImage.FileInfo.FullName) > File.GetLastWriteTime(secondImage.FileInfo.FullName))
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
        }

        if (posterImage == null || fanartImage == null)
            return Result.Failure<DetectPosterAndFanartImagesResponse>("Es konnten keine Poster- und Fanart-Bilddateien ermittelt werden.");

        return new DetectPosterAndFanartImagesResponse(posterImage, fanartImage);
    }
}

public record DetectPosterAndFanartImagesResponse(SupportedImage PosterImage, SupportedImage FanartImage);