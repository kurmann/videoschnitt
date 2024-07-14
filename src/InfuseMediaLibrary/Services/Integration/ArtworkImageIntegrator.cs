using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Services.FileSystem.Unix;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.Integration;

/// <summary>
/// Verwantwortlich für die Integration von Bildern in die lokale Infuse-Mediathek für die Anzeige als Titelbilder auf dem Infuse-Medienserver.
/// </summary>
internal class ArtworkImageIntegrator
{
    private readonly ILogger<ArtworkImageIntegrator> _logger;
    private readonly InfuseMediaLibrarySettings _infuseMediaLibrarySettings;
    private readonly IFileOperations _fileOperations;
    private readonly ArtworkDirectoryReader _artworkDirectoryReader;

    public ArtworkImageIntegrator(ILogger<ArtworkImageIntegrator> logger,
        IOptions<InfuseMediaLibrarySettings> infuseMediaLibrarySettings,
        IFileOperations fileOperations, ArtworkDirectoryReader artworkDirectoryReader)
    {
        _logger = logger;
        _infuseMediaLibrarySettings = infuseMediaLibrarySettings.Value;
        _fileOperations = fileOperations;
        _artworkDirectoryReader = artworkDirectoryReader;
    }

    /// <summary>
    /// Integriert die Bild-Dateien eines Mediensets in die Infuse-Mediathek in das gleiche Verzeichnis wie die Videodatei.
    /// </summary>
    /// <param name="artworkDirectory"></param>
    /// <param name="integratedVideo"></param>
    /// <returns></returns>
    public Task<Result> IntegrateImagesAsync(ArtworkDirectory artworkDirectory, SupportedVideo integratedVideo)
    {
        // Hole alle unterstützten Bild-Dateien aus dem Medienserver-Verzeichnis
        var supportedImagesResult = artworkDirectory.GetSupportedImages();
        if (supportedImagesResult.IsFailure)
        {
            return Task.FromResult(Result.Failure($"Fehler beim Ermitteln der unterstützten Bild-Dateien im Medienserver-Verzeichnis {artworkDirectory}. Fehler: {supportedImagesResult.Error}"));
        }

        // Todo: Der VideoFileTargetPath wird nur verwendet, um das Zielverzeichnis für die Bild-Dateien zu ermitteln. Es sollte stattdessen der Medienet-Name verwendet werden.
        // Integriere die Bild-Dateien in die Infuse-Mediathek
        return IntegrateImagesAsync(supportedImagesResult.Value, integratedVideo);
    }

    /// <summary>
    /// Verschiebt die unterstützten Bild-Dateien in das Infuse-Mediathek-Verzeichnis und konvertiert den Farbraum der Bilder in Adobe RGB.
    /// </summary>
    /// <param name="supportedImages"></param>
    /// <param name="integratedVideo"></param>
    /// <returns></returns>
    private async Task<Result> IntegrateImagesAsync(List<SupportedImage> supportedImages, SupportedVideo integratedVideo)
    {
        // Wenn kein Bild vorhanden sind, wird mit einer Info geloggt und die Methode beendet.
        if (supportedImages.Count == 0)
        {
            _logger.LogInformation("Es wurden keine unterstützten Bild-Dateien im Medienserver-Verzeichnis {mediaSetName.FullName} gefunden.", integratedVideo);
            return Result.Success();
        }

        // Ignoriere alle Bilddateien, die in Verwendung sind
        var supportedImagesNotInUse = new List<SupportedImage>();
        foreach (var supportedImage in supportedImages)
        {
            var isUsedResult = await _fileOperations.IsFileInUseAsync(supportedImage);
            if (isUsedResult.IsFailure)
            {
                return Result.Failure($"Fehler beim Prüfen, ob die Datei {supportedImage.Name} verwendet wird: {isUsedResult.Error}");
            }
            if (isUsedResult.Value)
            {
                _logger.LogInformation("Die Datei {File} wird derzeit verwendet und wird daher nicht in die Infuse-Mediathek integriert.", supportedImage.Name);
            }
            else
            {
                supportedImagesNotInUse.Add(supportedImage);
            }
        }
        supportedImages = supportedImagesNotInUse;

        // Ermittle das Zielverzeichnis für die Bild-Datei. Dieses ist das gleiche wie das Zielverzeichnis der Video-Datei.
        var videoTargetDirectory = integratedVideo.Directory;
        if (videoTargetDirectory == null)
        {
            return Result.Failure($"Das Verzeichnis der Video-Datei {integratedVideo} konnte nicht ermittelt werden. Das Verzeichnis wird benötigt, um die Bild-Dateien in das Infuse-Mediathek-Verzeichnis zu verschieben.");
        }

        // Lies den Inhalt des Verzeichnisses aus in dem die Bilder gespeichert sind
        var artworkDirectoryContentResult = _artworkDirectoryReader.GetDirectoryContent(videoTargetDirectory);
        if (artworkDirectoryContentResult.IsFailure)
        {
            return Result.Failure($"Das Verzeichnis '{videoTargetDirectory.FullName}' konnte nicht geöffnet werden: {artworkDirectoryContentResult.Error}");
        }

        // Wenn nur ein Bild vorhanden ist, wird dieses als Poster verwendet. Der Name des Bildes entspricht dem Namen der Video-Datei.
        if (supportedImages.Count == 1)
        {
            var supportedImage = supportedImages.First();

            var targetFilePath = Path.Combine(videoTargetDirectory.FullName, integratedVideo.Name.Replace(integratedVideo.Extension, supportedImage.Extension));
            var moveFileResult = await _fileOperations.CopyFileAsync(supportedImage, targetFilePath);
            if (moveFileResult.IsFailure)
            {
                return Result.Failure($"Die Bild-Datei {supportedImage} konnte nicht in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben werden. Fehler: {moveFileResult.Error}");
            }
            _logger.LogInformation("Bild-Datei {supportedImage.FileInfo.FullName} erfolgreich in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben.", supportedImage, videoTargetDirectory.FullName);
            return Result.Success();
        }

        // Wenn mehr als ein Bild vorhanden ist, dann werden die ersten zwei Bilder als Poster und Fanart verwendet und mit Hilfe des PosterAndFanartService die passenden Bilder ermittelt.
        var detectPosterAndFanartImagesResult = PosterAndFanartService.DetectPosterAndFanartImages(supportedImages.ElementAt(0), supportedImages.ElementAt(1));
        if (detectPosterAndFanartImagesResult.IsFailure)
        {
            return Result.Failure($"Das Poster und Fanart konnte nicht ermittelt werden: {detectPosterAndFanartImagesResult.Error}");
        }
        _logger.LogInformation($"Das Poster und Fanart wurde erfolgreich ermittelt.");
        _logger.LogInformation("Poster: {Name}", detectPosterAndFanartImagesResult.Value.PosterImage.Name);
        _logger.LogInformation("Fanart: {Name}", detectPosterAndFanartImagesResult.Value.FanartImage.Name);

        // Das Posterbild hat den gleichen Dateinamen die Videodatei.
        var posterImage = detectPosterAndFanartImagesResult.Value.PosterImage;
        var targetPosterFilePath = Path.Combine(videoTargetDirectory.FullName, integratedVideo.Name.Replace(integratedVideo.Extension, posterImage.Extension));

        // Prüfe ob bereits eine Datei am Zielort existiert mit gleichem Namen und gleichem Änderungsdatum
        if (FileOperations.ExistAtTarget(posterImage.FullName, targetPosterFilePath))
        {
            _logger.LogTrace("Die Poster-Datei {posterImage.FileInfo.FullName} existiert bereits im Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName}. Die Datei wird nicht verschoben.", posterImage.FullName, videoTargetDirectory.FullName);
        }
        else
        {
            // Kopiere das Posterbild in das Infuse-Mediathek-Verzeichnis
            var movePosterFileResult = await _fileOperations.CopyFileAsync(posterImage.FullName, targetPosterFilePath);
            if (movePosterFileResult.IsFailure)
            {
                return Result.Failure($"Das Posterbild {posterImage.FullName} konnte nicht in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben werden. Fehler: {movePosterFileResult.Error}");
            }
            _logger.LogInformation("Posterbild {FileName} erfolgreich in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben.", posterImage.Name, videoTargetDirectory.FullName);
        }

        // Das Fanartbild hat den gleichen Dateinamen wie die Videodatei und zusätzlich dem Postfix definiert aus den Einstellungen.
        var bannerFilePostfix = _infuseMediaLibrarySettings.BannerFilePostfix;
        if (string.IsNullOrWhiteSpace(bannerFilePostfix))
        {
            return Result.Failure("Das Suffix des Dateinamens, das für die Banner-Datei verwendet wird für die Infuse-Mediathek als Titelbild, ist nicht definiert.");
        }
        var fanartImage = detectPosterAndFanartImagesResult.Value.FanartImage;
        var targetFanartFilePath = Path.Combine(videoTargetDirectory.FullName, integratedVideo.Name.Replace(integratedVideo.Extension, $"{bannerFilePostfix}{fanartImage.Extension}"));

        // Prüfe ob bereits eine Datei am Zielort existiert mit gleichem Namen und gleichem Änderungsdatum
        if (FileOperations.ExistAtTarget(fanartImage.FullName, targetFanartFilePath))
        {
            _logger.LogTrace("Die Fanart-Datei {fanartImage.FileInfo.FullName} existiert bereits im Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName}. Die Datei wird nicht verschoben.", fanartImage.FullName, videoTargetDirectory.FullName);
        }
        else
        {
            // Kopiere das Fanartbild in das Infuse-Mediathek-Verzeichnis
            var moveFanartFileResult = await _fileOperations.CopyFileAsync(fanartImage.FullName, targetFanartFilePath);
            if (moveFanartFileResult.IsFailure)
            {
                return Result.Failure($"Das Fanartbild {fanartImage.FullName} konnte nicht in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben werden. Fehler: {moveFanartFileResult.Error}");
            }
            _logger.LogInformation("Fanartbild {fanartImage.FileInfo.FullName} erfolgreich in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben.", fanartImage.FullName, videoTargetDirectory.FullName);
        }

        return Result.Success();
    }
}
