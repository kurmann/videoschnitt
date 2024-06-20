using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

/// <summary>
/// Verantwortlich für das Erkennen des Medientyps.
public class MediaTypeDetectorService
{
    private readonly ILogger<MediaTypeDetectorService> _logger;

    public MediaTypeDetectorService(ILogger<MediaTypeDetectorService> logger) => _logger = logger;

    public Result<ISupportedMediaType> DetectMediaType(FileInfo fileInfo)
    {
        _logger.LogInformation($"Detecting media type for file {fileInfo.FullName}");

        var mpeg4Video = Mpeg4Video.Create(fileInfo);
        if (mpeg4Video.IsSuccess)
        {
            return mpeg4Video.Value;
        }

        var quickTimeMovie = QuickTimeMovie.Create(fileInfo);
        if (quickTimeMovie.IsSuccess)
        {
            return quickTimeMovie.Value;
        }

        var coverArtImage = SupportedImage.Create(fileInfo);
        if (coverArtImage.IsSuccess)
        {
            return coverArtImage.Value;
        }

        return Result.Failure<ISupportedMediaType>($"File {fileInfo.FullName} is not a supported media type.");
    }
}
