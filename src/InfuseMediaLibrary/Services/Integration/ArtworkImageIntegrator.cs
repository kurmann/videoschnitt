using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Services.FileSystem.Unix;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
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
    private readonly PosterAndFanartService _posterAndFanartService;

    public ArtworkImageIntegrator(ILogger<ArtworkImageIntegrator> logger,
        IOptions<InfuseMediaLibrarySettings> infuseMediaLibrarySettings,
        IFileOperations fileOperations,
        PosterAndFanartService posterAndFanartService)
    {
        _logger = logger;
        _infuseMediaLibrarySettings = infuseMediaLibrarySettings.Value;
        _fileOperations = fileOperations;
        _posterAndFanartService = posterAndFanartService;
    }

    /// <summary>
    /// Verschiebt die unterstützten Bild-Dateien in das Infuse-Mediathek-Verzeichnis und konvertiert den Farbraum der Bilder in Adobe RGB.
    /// </summary>
    /// <param name="supportedImages"></param>
    /// <param name="videoFileTargetPath"></param>
    /// <returns></returns>
    public async Task<Result> IntegrateImagesAsync(List<FileInfo> supportedImages, FileInfo videoFileTargetPath)
    {
        // Wenn kein Bild vorhanden sind, wird mit einer Info geloggt und die Methode beendet.
        if (supportedImages.Count == 0)
        {
            _logger.LogInformation($"Keine Bild-Dateien für das Medienset vorhanden.");
            _logger.LogInformation("Es wird kein Bild in das Infuse-Mediathek-Verzeichnis verschoben.");
            return Result.Success();
        }

        // Ermittle das Zielverzeichnis für die Bild-Datei. Dieses ist das gleiche wie das Zielverzeichnis der Video-Datei.
        var videoTargetDirectory = videoFileTargetPath.Directory;
        if (videoTargetDirectory == null)
        {
            return Result.Failure($"Das Verzeichnis der Video-Datei {videoFileTargetPath.FullName} konnte nicht ermittelt werden. Das Verzeichnis wird benötigt, um die Bild-Dateien in das Infuse-Mediathek-Verzeichnis zu verschieben.");
        }

        // Wenn nur ein Bild vorhanden ist, wird dieses als Poster verwendet. Der Name des Bildes entspricht dem Namen der Video-Datei.
        if (supportedImages.Count == 1)
        {
            var supportedImage = supportedImages.First();

            var targetFilePath = Path.Combine(videoTargetDirectory.FullName, videoFileTargetPath.Name.Replace(videoFileTargetPath.Extension, supportedImage.Extension));
            var moveFileResult = await _fileOperations.CopyFileAsync(supportedImage.FullName, targetFilePath);
            if (moveFileResult.IsFailure)
            {
                return Result.Failure($"Die Bild-Datei {supportedImage.FullName} konnte nicht in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben werden. Fehler: {moveFileResult.Error}");
            }
            _logger.LogInformation("Bild-Datei {supportedImage.FileInfo.FullName} erfolgreich in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben.", supportedImage.FullName, videoTargetDirectory.FullName);
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
        var targetPosterFilePath = Path.Combine(videoTargetDirectory.FullName, videoFileTargetPath.Name.Replace(videoFileTargetPath.Extension, posterImage.Extension));

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
        var targetFanartFilePath = Path.Combine(videoTargetDirectory.FullName, videoFileTargetPath.Name.Replace(videoFileTargetPath.Extension, $"{bannerFilePostfix}{fanartImage.Extension}"));

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
