using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

/// <summary>
/// Verantwortlich für das Erkennen des Mediensets.
/// </summary>
public class MediaSetService
{
    private readonly ModuleSettings _moduleSettings;
    private readonly ILogger<MediaSetService> _logger;
    private readonly FFmpegMetadataService _fFmpegMetadataService;

    public MediaSetService(FFmpegMetadataService fFmpegMetadataService, IOptions<ModuleSettings> moduleSettings, ILogger<MediaSetService> logger)
    {
        _fFmpegMetadataService = fFmpegMetadataService;
        _moduleSettings = moduleSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Analysiert das Verzeichnis und gruppiert die Medien-Dateien in Mediensets.
    public async Task<Result<List<MediaFilesByMediaSet>>> GroupToMediaSets(string directory)
    {
        // Prüfe ob ein Verzeichnis angegeben wurde
        if (string.IsNullOrWhiteSpace(directory))
        {
            return Result.Failure<List<MediaFilesByMediaSet>>("Das Verzeichnis darf nicht leer sein.");
        }

        try
        {
            return await GroupToMediaSets(new DirectoryInfo(directory));
        }
        catch (Exception ex)
        {
            return Result.Failure<List<MediaFilesByMediaSet>>($"Fehler beim Gruppieren der Medien-Dateien in Mediensets: {ex.Message}");
        }
    }

    public async Task<Result<List<MediaFilesByMediaSet>>> GroupToMediaSets(DirectoryInfo directoryInfo)
    {
        _logger.LogInformation("Versuche die Dateien im Verzeichnis in Medienset zu organisieren.");
        _logger.LogInformation("Prüfe ob das Verzeichnis existiert.");
        if (!directoryInfo.Exists)
        {
            return Result.Failure<List<MediaFilesByMediaSet>>($"Das Verzeichnis {directoryInfo.FullName} existiert nicht.");
        }

        _logger.LogInformation("Prüfe ob das Verzeichnis Dateien enthält.");
        _logger.LogInformation("Unterverzeichnisse werden nicht berücksichtigt.");
        var files = directoryInfo.GetFiles();
        if (files.Length == 0)
        {
            return Result.Failure<List<MediaFilesByMediaSet>>($"Das Verzeichnis {directoryInfo.FullName} enthält keine Dateien.");
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
            return Result.Failure<List<MediaFilesByMediaSet>>($"Fehler beim Lesen der Metadaten: {metadataResults.First(x => x.TitleResult.IsFailure).TitleResult.Error}");
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
        _logger.LogInformation("Gruppierung der Medien-Dateien in Mediensets erfolgreich.");
        _logger.LogInformation("Anzahl Mediensets: {Count}", mediaFilesByMediaSet.Count);

        foreach (var mediaFiles in mediaFilesByMediaSet)
        {
            _logger.LogInformation("Medienset: {Title}", mediaFiles.Title);
            _logger.LogInformation("Anzahl Videos: {Count}", mediaFiles.VideoFiles.Count());
            _logger.LogInformation("Anzahl Bilder: {Count}", mediaFiles.ImageFiles.Count());
        }

        return Result.Success(mediaFilesByMediaSet);
    }
}

/// <summary>
/// Repräsentiert eine Gruppierung von unterstützten Videodateien nach Medienset.
/// </summary>
/// <param name="Title"></param>
/// <param name="VideoFiles"></param>
/// <returns></returns>
public record VideosByMediaSet(string Title, IEnumerable<SupportedVideo> VideoFiles);

/// <summary>
/// Repräsentiert eine Gruppierung von separat gelisteten unterstützten Medien-Dateien nach Medienset.
/// </summary>
/// <param name="Title"></param>
/// <param name="VideoFiles"></param>
/// <param name="ImageFiles"></param>
/// <returns></returns>
public record MediaFilesByMediaSet(string Title, IEnumerable<SupportedVideo> VideoFiles, IEnumerable<SupportedImage> ImageFiles);