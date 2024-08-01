using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Services.FileSystem.Unix;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.LocalIntegration;

/// <summary>
/// Verwantwortlich für die Integration von Bildern in die lokale Infuse-Mediathek für die Anzeige als Titelbilder auf dem Infuse-Medienserver.
/// </summary>
internal class ArtworkImageIntegrator
{
    private readonly ILogger<ArtworkImageIntegrator> _logger;
    private readonly InfuseMediaLibrarySettings _infuseMediaLibrarySettings;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;
    private readonly IFileOperations _fileOperations;

    public ArtworkImageIntegrator(ILogger<ArtworkImageIntegrator> logger, IOptions<InfuseMediaLibrarySettings> infuseMediaLibrarySettings, IFileOperations fileOperations, IOptions<MediaSetOrganizerSettings> mediaSetOrganizerSettings)
    {
        _logger = logger;
        _infuseMediaLibrarySettings = infuseMediaLibrarySettings.Value;
        _fileOperations = fileOperations;
        _mediaSetOrganizerSettings = mediaSetOrganizerSettings.Value;
    }

    /// <summary>
    /// Integriert die Bild-Dateien eines Mediensets in die Infuse-Mediathek in das gleiche Verzeichnis wie die Videodatei.
    /// </summary>
    /// <param name="artworkDirectory"></param>
    /// <param name="integratedVideo"></param>
    /// <returns></returns>
    public Task<Result> IntegrateImagesAsync(ArtworkDirectory? artworkDirectory, SupportedVideo integratedVideo)
    {
        // Wenn kein Artwork-Verzeichnis vorhanden ist, wird mit einer Info geloggt und die Methode beendet.
        if (artworkDirectory == null)
        {
            _logger.LogInformation("Es wurde kein Artwork-Verzeichnis für die Videodatei {integratedVideo} gefunden. Es werden keine unterstützten Bild-Dateien in die Infuse-Mediathek integriert.", integratedVideo);
            return Task.FromResult(Result.Success());
        }

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
            _logger.LogInformation("Es wurden keine unterstützten Bild-Dateien im Medienserver-Verzeichnis {mediaSetName.FullName} gefunden, die für Titelbilder verwendet werden können.", integratedVideo);
            return Result.Success();
        }

        // Ignoriere alle Bilddateien, die in Verwendung sind
        var removeImagesResult = await RemoveImagesInUse(supportedImages);
        if (removeImagesResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Entfernen der Bild-Dateien, die in Verwendung sind: {removeImagesResult.Error}");
        }

        var posterImage = GetPosterImage(supportedImages);
        var fanartImage = GetFanartImage(supportedImages);

        // Keine Poster- und Fanart-Bilder identifiziert
        if (posterImage.HasNoValue && fanartImage.HasNoValue)
        {
            // Warne, dass wir bei zwei Bildern keine Poster- und Fanart-Bilder identifizieren konnten.
            _logger.LogWarning("Es wurden keine Poster- und Fanart-Bilder identifiziert. Es wurden zwei unterstützte Bilder gefunden, aber keines davon entspricht den Suffixen für Poster- und Fanart-Bilder.");
        }

        // Poster- und Fanart-Bilder identifiziert
        if (posterImage.HasValue && fanartImage.HasValue)
        {
            // Informiere, dass wir anhand des Dateisuffixes das Poster- und Fanart-Bild identifiziert haben.
            _logger.LogInformation("Das Poster- und Fanart-Bild wurden anhand des Dateisuffixes identifiziert: Poster-Bild: '{posterImage.Name}', Fanart-Bild: '{fanartImage.Name}'", posterImage.Value.Name, fanartImage.Value.Name);
            await IntegratePosterImage(integratedVideo, posterImage);
            await IntegrateFanartImage(integratedVideo, fanartImage);
        }

        // Nur Poster-Bild identifiziert
        if (posterImage.HasValue && fanartImage.HasNoValue)
        {
            // Informiere, dass wir anhand des Dateisuffixes nur das Poster-Bild identifiziert haben. Es wird kein Fanart-Bild verwendet.
            _logger.LogInformation("Das Poster-Bild wurde anhand des Dateisuffixes identifiziert: Poster-Bild: '{posterImage.Name}'", posterImage.Value.Name);
            _logger.LogInformation("Das Fanart-Bild wird nicht in die Infuse-Mediathek integriert, da kein passendes Bild gefunden wurde.");
            await IntegratePosterImage(integratedVideo, posterImage);
        }

        // Nur Fanart-Bild identifiziert
        if (fanartImage.HasValue && posterImage.HasNoValue)
        {
            // Informiere, dass wir anhand des Dateisuffixes nur das Fanart-Bild identifiziert haben. Dieses wird gleichzeitig auch für das Poster-Bild verwendet.
            _logger.LogInformation("Das Fanart-Bild wurde anhand des Dateisuffixes identifiziert: Fanart-Bild: '{fanartImage.Name}'", fanartImage.Value.Name);
            _logger.LogInformation("Das Poster-Bild wird von dem Fanart-Bild abgeleitet.");
            await IntegrateFanartImage(integratedVideo, fanartImage);
            await IntegratePosterImage(integratedVideo, fanartImage);
        }

        return Result.Success();
    }

    private async Task<Result> IntegrateFanartImage(SupportedVideo integratedVideo, Maybe<SupportedImage> fanartImage)
    {
        // Ermittle das Zielverzeichnis für die Bild-Datei. Dieses ist das gleiche wie das Zielverzeichnis der Video-Datei, die in vorherigen Schritten integriert wurde.
        var videoTargetDirectory = integratedVideo.Directory;
        if (videoTargetDirectory == null)
        {
            return Result.Failure($"Das Verzeichnis der Video-Datei {integratedVideo} konnte nicht ermittelt werden. Das Verzeichnis wird benötigt, um die Bild-Dateien in das Infuse-Mediathek-Verzeichnis zu verschieben.");
        }

        // Prüfe, ob für das Fanart-Bild eine Version in Adobe RGB existiert.
        if (fanartImage.Value.FileInfoAdobeRgb.HasNoValue)
        {
            return Result.Failure($"Für das Fanart-Bild {fanartImage.Value.Name} konnte keine Version in Adobe RGB gefunden werden.");
        }

        // Das Fanartbild hat den gleichen Dateinamen wie die Videodatei und zusätzlich dem Postfix definiert aus den Einstellungen.
        var bannerFilePostfix = _infuseMediaLibrarySettings.BannerFilePostfix;
        if (string.IsNullOrWhiteSpace(bannerFilePostfix))
        {
            return Result.Failure("Das Suffix des Dateinamens, das für die Banner-Datei verwendet wird für die Infuse-Mediathek als Titelbild, ist nicht definiert.");
        }
        var targetFanartFilePath = Path.Combine(videoTargetDirectory.FullName, integratedVideo.Name.Replace(integratedVideo.Extension, $"{bannerFilePostfix}{fanartImage.Value.ExtensionAdobeRgb.Value}"));

        // Prüfe ob bereits eine Datei am Zielort existiert mit gleichem Namen und gleichem Änderungsdatum
        if (FileOperations.ExistAtTarget(fanartImage.Value, targetFanartFilePath))
        {
            _logger.LogTrace("Die Fanart-Datei {fanartImage.FileInfo.FullName} existiert bereits im Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName}. Die Datei wird nicht verschoben.", fanartImage.Value, videoTargetDirectory.FullName);
        }
        else
        {
            // Kopiere das Fanartbild in das Infuse-Mediathek-Verzeichnis
            var moveFanartFileResult = await _fileOperations.CopyFileAsync(fanartImage.Value.FileInfoAdobeRgb.Value.FullName, targetFanartFilePath, true, false);
            if (moveFanartFileResult.IsFailure)
            {
                return Result.Failure($"Das Fanartbild {fanartImage.Value} konnte nicht in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben werden. Fehler: {moveFanartFileResult.Error}");
            }
            _logger.LogInformation("Fanartbild {fanartImage.FileInfo.FullName} erfolgreich in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben.", fanartImage.Value, videoTargetDirectory.FullName);
        }

        return Result.Success();
    }

    private async Task<Result> IntegratePosterImage(SupportedVideo integratedVideo, Maybe<SupportedImage> posterImage)
    {
        // Ermittle das Zielverzeichnis für die Bild-Datei. Dieses ist das gleiche wie das Zielverzeichnis der Video-Datei, die in vorherigen Schritten integriert wurde.
        var videoTargetDirectory = integratedVideo.Directory;
        if (videoTargetDirectory == null)
        {
            return Result.Failure($"Das Verzeichnis der Video-Datei {integratedVideo} konnte nicht ermittelt werden. Das Verzeichnis wird benötigt, um die Bild-Dateien in das Infuse-Mediathek-Verzeichnis zu verschieben.");
        }

        // Prüfe, falls ein Poster-Image existiert, ob für das Poster-Bild eine Version in Adobe RGB existiert. 
        if (posterImage.HasValue && posterImage.Value.FileInfoAdobeRgb.HasNoValue)
        {
            return Result.Failure($"Für das Poster-Bild {posterImage.Value.Name} konnte keine Version in Adobe RGB gefunden werden.");
        }

        // Das Posterbild hat den gleichen Dateinamen die Videodatei.
        var targetPosterFilePath = Path.Combine(videoTargetDirectory.FullName, integratedVideo.Name.Replace(integratedVideo.Extension, posterImage.Value.ExtensionAdobeRgb.Value));

        // Prüfe ob bereits eine Datei am Zielort existiert mit gleichem Namen und gleichem Änderungsdatum
        if (FileOperations.ExistAtTarget(posterImage.Value, targetPosterFilePath))
        {
            _logger.LogTrace("Die Poster-Datei {posterImage.FileInfo.FullName} existiert bereits im Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName}. Die Datei wird nicht verschoben.", posterImage.Value, videoTargetDirectory.FullName);
        }
        else
        {
            // Kopiere das Posterbild in das Infuse-Mediathek-Verzeichnis
            var movePosterFileResult = await _fileOperations.CopyFileAsync(posterImage.Value.FileInfoAdobeRgb.Value.FullName, targetPosterFilePath, true, false);
            if (movePosterFileResult.IsFailure)
            {
                return Result.Failure($"Das Posterbild {posterImage.Value.FileInfoAdobeRgb.Value.FullName} konnte nicht in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben werden. Fehler: {movePosterFileResult.Error}");
            }
            _logger.LogInformation("Posterbild {FileName} erfolgreich in das Infuse-Mediathek-Verzeichnis {videoTargetDirectory.FullName} verschoben.", posterImage.Value.Name, videoTargetDirectory.FullName);
        }

        return Result.Success();
    }

    private Maybe<SupportedImage> GetFanartImage(List<SupportedImage> supportedImages)
    {
        // Suche unter den unterstützten Bildern das Fanart-Bild (mit dem entsprechenden Suffix im Dateinamen)
        return supportedImages.FirstOrDefault(x => _mediaSetOrganizerSettings.MediaSet.OrientationSuffixes.HasLandscapeSuffix(x)) ?? Maybe<SupportedImage>.None;
    }

    private Maybe<SupportedImage> GetPosterImage(List<SupportedImage> supportedImages)
    {
        // Suche unter den unterstützten Bildern das Poster-Bild (ohne Suffix im Dateinamen)
        return supportedImages.FirstOrDefault(x => _mediaSetOrganizerSettings.MediaSet.OrientationSuffixes.HasPortraitSuffix(x)) ?? Maybe<SupportedImage>.None;
    }

    private async Task<Result> RemoveImagesInUse(List<SupportedImage> supportedImages)
    {
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

                // entferne die Datei aus der Liste der unterstützten Bilder
                supportedImages.Remove(supportedImage);
            }
        }
        return Result.Success();
    }
}
