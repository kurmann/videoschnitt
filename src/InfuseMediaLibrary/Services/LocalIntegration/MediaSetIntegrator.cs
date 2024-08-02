using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
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

    internal async Task<Result<Maybe<IntegratedLocalInfuseMediaSet>>> IntegrateMediaSetAsync(MediaSetDirectory mediaSetDirectory)
    {
        // Integriere die Medienserver-Datei aus dem Medienset in die Infuse-Mediathek
        var integratedVideoResult = await _videoIntegrator.IntegrateMediaServerFiles(mediaSetDirectory.MediaServerFilesDirectory.GetValueOrDefault());
        if (integratedVideoResult.IsFailure)
        {
            return Result.Failure<Maybe<IntegratedLocalInfuseMediaSet>>($"Fehler beim Integrieren der Videodatei in die Infuse-Mediathek: {integratedVideoResult.Error}");
        }

        // Wenn keine Videodatei gefunden wurde, ergibt es keinen Sinn, weitere Integrationsschritte durchzuführen
        var integratedVideo = Maybe<SupportedVideo>.None;
        if (integratedVideoResult.Value.HasNoValue)
        {
            return Maybe<IntegratedLocalInfuseMediaSet>.None;
        }
        else 
        {
            _logger.LogInformation("Videodatei {Video} wurde erfolgreich in die Infuse-Mediathek integriert.", integratedVideoResult.Value);
            integratedVideo = integratedVideoResult.Value;
        }

        // Integriere die Titelbilder in die Infuse-Mediathek
        var integrateArtworkImagesTask = await _artworkImageIntegrator.IntegrateImagesAsync(mediaSetDirectory.ArtworkDirectory.GetValueOrDefault(), integratedVideo.Value);
        if (integrateArtworkImagesTask.IsFailure)
        {
            // Logge eine Warnung, aber fahre mit der Integration fort
            _logger.LogWarning("Fehler beim Integrieren der Artwork-Bilder in die Infuse-Mediathek: {Error}", integrateArtworkImagesTask.Error);
        }
        else
        {
            _logger.LogInformation("Artwork-Bilder für die Videodatei {Video} wurden erfolgreich in die Infuse-Mediathek integriert.", integratedVideo);
        }

        // Integriere die Metadaten-XML-Datei in die Infuse-Mediathek
        var integrateMetadataResult = await _metadataFileIntegrator.IntegrateMetadataFileAsync(mediaSetDirectory, integratedVideo.Value);
        if (integrateMetadataResult.IsFailure)
        {
            // Logge eine Warnung, aber fahre mit der Integration fort
            _logger.LogWarning("Fehler beim Integrieren der Metadaten-XML-Datei in die Infuse-Mediathek: {Error}", integrateMetadataResult.Error);
        }
        else
        {
            _logger.LogInformation("Metadaten-XML-Datei für die Videodatei {Video} wurde erfolgreich in die Infuse-Mediathek integriert.", integratedVideo);
        }

        var integratedLocalInfuseMedia = new IntegratedLocalInfuseMediaSet(integratedVideo, Maybe<List<SupportedImage>>.None, Maybe<InfuseMetadataXmlFile>.None);
        return Maybe<IntegratedLocalInfuseMediaSet>.From(integratedLocalInfuseMedia);
    }
}

internal record IntegratedLocalInfuseMediaSet(Maybe<SupportedVideo> Video, Maybe<List<SupportedImage>> ArtworkImages, Maybe<InfuseMetadataXmlFile> MetadataFile);
