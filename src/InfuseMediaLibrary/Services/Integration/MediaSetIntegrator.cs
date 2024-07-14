using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.Integration;

/// <summary>
/// Verantwortlich für die Integration von Mediensets in die Infuse-Mediathek
/// </summary>
internal class MediaSetIntegrator
{
    private readonly ArtworkImageIntegrator _artworkImageIntegrator;
    private readonly VideoIntegrator _videoIntegrator;
    private readonly ILogger<MediaSetIntegrator> _logger;

    public MediaSetIntegrator(ArtworkImageIntegrator artworkImageIntegrator, VideoIntegrator videoIntegrator, ILogger<MediaSetIntegrator> logger)
    {
        _artworkImageIntegrator = artworkImageIntegrator;
        _videoIntegrator = videoIntegrator;
        _logger = logger;
    }

    internal async Task<Result> IntegrateMediaSetAsync(MediaSetDirectory mediaSetDirectory)
    {
        var integratedVideoResult = await _videoIntegrator.IntegrateMediaServerFiles(mediaSetDirectory.MediaServerFilesDirectory.GetValueOrDefault());
        if (integratedVideoResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Integrieren der Videodatei in die Infuse-Mediathek: {integratedVideoResult.Error}");
        }
        _logger.LogInformation("Videodatei {Video} wurde erfolgreich in die Infuse-Mediathek integriert.", integratedVideoResult.Value);
        var integratedVideo = integratedVideoResult.Value;

        var integrateArtworkImagesTask = await _artworkImageIntegrator.IntegrateImagesAsync(mediaSetDirectory.ArtworkDirectory.GetValueOrDefault(), integratedVideo);
        if (integrateArtworkImagesTask.IsFailure)
        {
            return Result.Failure($"Fehler beim Integrieren der Artwork-Bilder in die Infuse-Mediathek: {integrateArtworkImagesTask.Error}");
        }
        _logger.LogInformation("Artwork-Bilder für die Videodatei {Video} wurden erfolgreich in die Infuse-Mediathek integriert.", integratedVideo);

        return Result.Success();
    }
}
