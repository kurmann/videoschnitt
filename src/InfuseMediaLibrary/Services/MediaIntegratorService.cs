using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.Integration;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

/// <summary>
/// Verantwortlich für die Integration von Mediensets in die lokale Infuse-Mediathek.
/// </summary>
internal class MediaIntegratorService
{
    private readonly ArtworkImageIntegrator _artworkImageIntegrator;
    private readonly VideoIntegratorService _videoIntegratorService;
    private readonly ILogger<MediaIntegratorService> _logger;

    public MediaIntegratorService(ILogger<MediaIntegratorService> logger, 
        ArtworkImageIntegrator artworkImageIntegrator, VideoIntegratorService videoIntegratorService)
    {
        _logger = logger;
        _artworkImageIntegrator = artworkImageIntegrator;
        _videoIntegratorService = videoIntegratorService;
    }

    public async Task<Result> IntegrateToLocalInfuseMediaLibrary(FileInfo videoFile, IEnumerable<FileInfo> imageFiles)
    {
        _logger.LogInformation("Intergriere alle Videos im Medienserver-Verzeichnis und alle zugehörigen Bilder in die lokale Infuse-Mediathek.");
        var videoIntegrationResultTask = _videoIntegratorService.IntegrateVideoAsync(videoFile);
        var imageIntegrationResultTask = _artworkImageIntegrator.IntegrateImagesAsync(imageFiles, videoFile);

        var integrationResults = await Task.WhenAll(videoIntegrationResultTask, imageIntegrationResultTask);
        var results = Result.Combine(integrationResults);
        if (results.IsFailure)
        {
            return Result.Failure<Maybe<LocalMediaServerFiles>>($"Die Integration der Medienset-Dateien in das Infuse-Mediathek-Verzeichnis war nicht erfolgreich: {results.Error}");
        }

        _logger.LogInformation("Die Integration der Medienset-Dateien in das Infuse-Mediathek-Verzeichnis war erfolgreich.");

        return Result.Success();
    }
}
