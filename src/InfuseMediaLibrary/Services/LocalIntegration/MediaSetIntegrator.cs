using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.LocalIntegration;

/// <summary>
/// Verantwortlich für die Integration von Mediensets in die Infuse-Mediathek
/// </summary>
internal class MediaSetIntegrator
{
    private readonly ArtworkImageIntegrator _artworkImageIntegrator;
    private readonly VideoIntegrator _videoIntegrator;
    private readonly MetadataFileIntegrator _metadataFileIntegrator;
    private readonly ILogger<MediaSetIntegrator> _logger;

    public MediaSetIntegrator(ArtworkImageIntegrator artworkImageIntegrator, VideoIntegrator videoIntegrator, ILogger<MediaSetIntegrator> logger, MetadataFileIntegrator metadataFileIntegrator)
    {
        _artworkImageIntegrator = artworkImageIntegrator;
        _videoIntegrator = videoIntegrator;
        _logger = logger;
        _metadataFileIntegrator = metadataFileIntegrator;
    }

    internal async Task<Result> IntegrateMediaSetAsync(MediaSetDirectory mediaSetDirectory)
    {
        // Integriere die Medienserver-Datei aus dem Medienset in die Infuse-Mediathek
        var integratedVideoResult = await _videoIntegrator.IntegrateMediaServerFiles(mediaSetDirectory.MediaServerFilesDirectory.GetValueOrDefault());
        if (integratedVideoResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Integrieren der Videodatei in die Infuse-Mediathek: {integratedVideoResult.Error}");
        }
        _logger.LogInformation("Videodatei {Video} wurde erfolgreich in die Infuse-Mediathek integriert.", integratedVideoResult.Value);
        var integratedVideo = integratedVideoResult.Value;

        // Wenn keine Videodatei gefunden wurde, ergibt es keinen Sinn, weitere Integrationsschritte durchzuführen
        if (integratedVideo.HasNoValue)
        {
            return Result.Success();
        }

        // Integriere die Titelbilder in die Infuse-Mediathek
        var integrateArtworkImagesTask = await _artworkImageIntegrator.IntegrateImagesAsync(mediaSetDirectory.ArtworkDirectory.GetValueOrDefault(), integratedVideo.Value);
        if (integrateArtworkImagesTask.IsFailure)
        {
            return Result.Failure($"Fehler beim Integrieren der Artwork-Bilder in die Infuse-Mediathek: {integrateArtworkImagesTask.Error}");
        }
        _logger.LogInformation("Artwork-Bilder für die Videodatei {Video} wurden erfolgreich in die Infuse-Mediathek integriert.", integratedVideo);

        // Integriere die Metadaten-XML-Datei in die Infuse-Mediathek
        var integrateMetadataResult = await _metadataFileIntegrator.IntegrateMetadataFileAsync(mediaSetDirectory, integratedVideo.Value);
        if (integrateMetadataResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Integrieren der Metadaten-Datei in die Infuse-Mediathek: {integrateMetadataResult.Error}");
        }
        _logger.LogInformation("Infuse Metadaten-XML-Datei wurde erfolgreich in die Infuse-Mediathek integriert.");

        return Result.Success();
    }
}
