using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.MetadataProcessor.Entities;
using Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

/// <summary>
/// Verantwortlich für das Erkennen des Mediensets.
/// </summary>
public class MediaSetService
{
    private readonly MediaSetVariantService _mediaSetVariantService;
    private readonly ModuleSettings _moduleSettings;
    private readonly ILogger<MediaSetService> _logger;
    private readonly FFmpegMetadataService _fFmpegMetadataService;

    public MediaSetService(MediaSetVariantService mediaSetVariantService, FFmpegMetadataService fFmpegMetadataService, IOptions<ModuleSettings> moduleSettings, ILogger<MediaSetService> logger)
    {
        _mediaSetVariantService = mediaSetVariantService;
        _fFmpegMetadataService = fFmpegMetadataService;
        _moduleSettings = moduleSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Analysiert das Verzeichnis und gruppiert die Medien-Dateien in Mediensets.
    public async Task<Result<List<MediaSet>>> GroupToMediaSets(string directory)
    {
        // Prüfe ob ein Verzeichnis angegeben wurde
        if (string.IsNullOrWhiteSpace(directory))
        {
            return Result.Failure<List<MediaSet>>("Das Verzeichnis darf nicht leer sein.");
        }

        try
        {
            return await GroupToMediaSets(new DirectoryInfo(directory));
        }
        catch (Exception ex)
        {
            return Result.Failure<List<MediaSet>>($"Fehler beim Gruppieren der Medien-Dateien in Mediensets: {ex.Message}");
        }
    }

    public async Task<Result<List<MediaSet>>> GroupToMediaSets(DirectoryInfo directoryInfo)
    {
        _logger.LogInformation("Versuche die Dateien im Verzeichnis in Medienset zu organisieren.");

        _logger.LogInformation("Prüfe ob das Verzeichnis existiert.");
        if (!directoryInfo.Exists)
        {
            return Result.Failure<List<MediaSet>>($"Das Verzeichnis {directoryInfo.FullName} existiert nicht.");
        }

        _logger.LogInformation("Prüfe ob das Verzeichnis Dateien enthält.");
        var files = directoryInfo.GetFiles();
        if (files.Length == 0)
        {
            return Result.Failure<List<MediaSet>>($"Das Verzeichnis {directoryInfo.FullName} enthält keine Dateien.");
        }

        _logger.LogInformation("Filtere nach unterstützten Medien-Dateien. Diese sind grundsätzlich QuickTime-Movie-Dateien oder MPEG4-Dateien.");
        var videoFileInfos = files.Where(f => SupportedVideo.IsSupportedVideoExtension(f)).ToList();
        var supportedVideoFiles = new List<SupportedVideo>();
        foreach (var videoFileInfo in videoFileInfos)
        {
            var supportedVideoResult = SupportedVideo.Create(videoFileInfo);
            if (supportedVideoResult.IsSuccess)
            {
                supportedVideoFiles.Add(supportedVideoResult.Value);
            }
            else
            {
                _logger.LogWarning($"Die Datei {videoFileInfo.FullName} ist keine unterstützte Videodatei: {supportedVideoResult.Error}");
            }
        }

        _logger.LogInformation("Lies aus allen unterstützen Videodateien mit FFMPeg den Titel-Tag aus den Metadaten und gruppiere alle Dateien mit dem gleichen Titel.");
        var metadataTasks = supportedVideoFiles
            .Select(async f => new
            {
                File = f,
                TitleResult = await _fFmpegMetadataService.GetMetadataFieldAsync(f.FileInfo, "title")
            });
        var metadataResults = await Task.WhenAll(metadataTasks);

        _logger.LogInformation("Prüfe ob die Metadaten bei allen Dateien erfolgreich gelesen werden konnten.");
        if (metadataResults.Any(x => x.TitleResult.IsFailure))
        {
            return Result.Failure<List<MediaSet>>($"Fehler beim Lesen der Metadaten: {metadataResults.First(x => x.TitleResult.IsFailure).TitleResult.Error}");
        }

        _logger.LogInformation("Gruppiere die Dateien nach Titel.");
        var videosByMediaSet = metadataResults
            .Where(x => x.TitleResult.IsSuccess)
            .GroupBy(x => x.TitleResult.Value)
            .Select(g => new VideosByMediaSet(g.Key, g.Select(x => x.File)));

        _logger.LogInformation("Suche in jedem Medienset ob noch eine unterstütze Bild-Datei vorhanden ist. Diese muss das gleiche Basis-Datei-Name haben wie die Videodatei.");
        _logger.LogInformation("Wenn ja, füge die Bild-Datei zum Medienset hinzu.");
        var mediaFilesByMediaSet = new List<MediaFilesByMediaSet>();
        foreach (var videos in videosByMediaSet)
        {
            var imageFileInfos = files.Where(f => SupportedImage.IsSupportedImageExtension(f) && f.Name.StartsWith(videos.Title, StringComparison.InvariantCultureIgnoreCase)).ToList();
            var supportedImageFiles = new List<SupportedImage>();
            foreach (var imageFileInfo in imageFileInfos)
            {
                var supportedImageResult = SupportedImage.Create(imageFileInfo);
                if (supportedImageResult.IsSuccess)
                {
                    supportedImageFiles.Add(supportedImageResult.Value);
                }
                else
                {
                    _logger.LogWarning($"Die Datei {imageFileInfo.FullName} ist keine unterstützte Bilddatei: {supportedImageResult.Error}");
                }
            }

            mediaFilesByMediaSet.Add(new MediaFilesByMediaSet(videos.Title, videos.VideoFiles, supportedImageFiles));
        }


        // todo: diese Nachfolgende Logik muss noch korrekt implementiert werden

        _logger.LogInformation("Prüfe ob das Verzeichnis unterstützte Medien-Dateien enthält.");
        var videoVersionSuffixesForMediaServer = _moduleSettings.MediaSet?.VideoVersionSuffixesForMediaServer;
        if (videoVersionSuffixesForMediaServer == null || videoVersionSuffixesForMediaServer.Count == 0)
        {
            return Result.Failure<List<MediaSet>>("Keine Suffixe für die Medienserver-Versionen konfiguriert. Medienserver-Versionen können nicht erkannt werden.");
        }

        _logger.LogInformation("Prüfe ob das Verzeichnis unterstützte Medien-Dateien enthält.");
        var mediaServerFiles = files.Where(f => videoVersionSuffixesForMediaServer.Any(s => f.Name.Contains(s, StringComparison.InvariantCultureIgnoreCase))).ToList();

        _logger.LogInformation("Liste der Medienserver-Dateien:");
        foreach (var mediaServerFile in mediaServerFiles)
        {
            _logger.LogInformation(mediaServerFile.FullName);
        }

        if (mediaServerFiles.Count == 0)
        {
            return Result.Failure<List<MediaSet>>($"Keine Medienserver-Dateien gefunden. Es wurden keine Dateien mit den Suffixen {string.Join(", ", videoVersionSuffixesForMediaServer)} gefunden.");
        }

        _logger.LogInformation("Prüfe ob nur eine Medienserver-Datei gefunden wurde.");
        if (mediaServerFiles.Count > 1)
        {
            return Result.Failure<List<MediaSet>>($"Mehrere Medienserver-Dateien gefunden. Es sollte nur eine Datei mit den Suffixen {string.Join(", ", videoVersionSuffixesForMediaServer)} gefunden werden.");
        }

        _logger.LogInformation("Prüfe ob die Medienserver-Datei eine gültige Medienserver-Datei ist.");
        var mediaServerFileResult = MediaServerFile.Create(mediaServerFiles.First().FullName);
        if (mediaServerFileResult.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Die Medienserver-Datei ist keine gültige Medienserver-Datei: {mediaServerFileResult.Error}");
        }

        var mediaSet = MediaSet.Create("Test", "TestAsFileName", "TestAsFilePath", mediaServerFileResult.Value);
        if (mediaSet.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Das Medienset konnte nicht erstellt werden: {mediaSet.Error}");
        }

        return Result.Success(new List<MediaSet> { mediaSet.Value });
    }
}

internal record VideosByMediaSet(string Title, IEnumerable<SupportedVideo> VideoFiles);

internal record MediaFilesByMediaSet(string Title, IEnumerable<SupportedVideo> VideoFiles, IEnumerable<SupportedImage> ImageFiles);