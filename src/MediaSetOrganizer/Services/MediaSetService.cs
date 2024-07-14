using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.Common.Services.Metadata;
using Kurmann.Videoschnitt.Common.Models;

namespace Kurmann.Videoschnitt.MediaSetOrganizer.Services;

/// <summary>
/// Verantwortlich für das Erkennen des Mediensets.
/// </summary>
public class MediaSetService
{
    private readonly ILogger<MediaSetService> _logger;
    private readonly FFmpegMetadataService _fFmpegMetadataService;

    public MediaSetService(FFmpegMetadataService fFmpegMetadataService, ILogger<MediaSetService> logger)
    {
        _fFmpegMetadataService = fFmpegMetadataService;
        _logger = logger;
    }

    /// <summary>
    /// Gib den Inhalt des Verzeichnisses als gruppierte Mediensets zurück.
    /// </summary>
    /// <param name="inputDirectoryContent"></param>
    /// <returns></returns>
    public async Task<Result<List<MediaFilesByMediaSet>>> GroupToMediaSets(InputDirectoryContent inputDirectoryContent)
    {
        // Prüfe ob überhaupt unterstützte Dateien im Verzeichnis sind
        if (inputDirectoryContent.SupportedImages.Count == 0 && inputDirectoryContent.SupportedVideos.Count == 0)
        {
            _logger.LogWarning("Keine unterstützten Dateien im Verzeichnis gefunden oder keine Dateien gefunden, die nicht in Verarbeitung sind.");
            return Result.Success(new List<MediaFilesByMediaSet>());
        }

        _logger.LogInformation("Versuche die Dateien im Verzeichnis in Medienset zu organisieren.");

        _logger.LogInformation("Lies aus allen unterstützen Videodateien mit FFMPeg den Titel-Tag aus den Metadaten und gruppiere alle Dateien mit dem gleichen Titel.");
        var metadataTasks = inputDirectoryContent.SupportedVideos
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

        _logger.LogInformation("Prüfe ob alle Dateien einen Titel-Tag haben.");
        var filesWithEmptyTitleTags = metadataResults.Where(x => string.IsNullOrWhiteSpace(x.TitleResult.Value)).Select(x => x.File.FileInfo).ToArray();

        if (filesWithEmptyTitleTags.Length != 0)
        {
            _logger.LogWarning("Folgende Dateien haben keinen Titel-Tag und werden ignoriert:");
            foreach (var file in filesWithEmptyTitleTags)
            {
                _logger.LogWarning("{FullName}", file.FullName);
            }
            
            _logger.LogInformation("Entferne alle Dateien die einen leeren Titel haben von der weiteren Verarbeitung.");
            metadataResults = metadataResults.Where(x => !string.IsNullOrWhiteSpace(x.TitleResult.Value)).ToArray();
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
            _logger.LogInformation("Suche nach allen unterstützten Bild-Dateien die das gleiche Basis-Datei-Name haben wie die Videodatei");
            var supportedImageFileInfos = inputDirectoryContent.SupportedImages.Select(f => f.FileInfo);
            var imageFileInfos = supportedImageFileInfos.Where(i => i.Name.StartsWith(videos.NameString)).ToArray();
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
                    _logger.LogWarning("Die Datei {FullName} ist keine unterstützte Bilddatei: {Error}", imageFileInfo.FullName, supportedImageResult.Error);
                }
            }

            _logger.LogInformation("Suche für jedes Medienset nach einer Masterdatei.");
            var masterfile = inputDirectoryContent.Masterfiles.FirstOrDefault(m => m.FileInfo.Name.StartsWith(videos.NameString));

            // Parse den Medienset-Namen
            var mediaSetNameResult = MediaSetName.Create(videos.NameString);
            if (mediaSetNameResult.IsFailure)
            {
                _logger.LogError("Fehler beim Parsen des Medienset-Namens '{MediaSetName}': {Error}", videos.NameString, mediaSetNameResult.Error);
                continue;
            }

            mediaFilesByMediaSet.Add(new MediaFilesByMediaSet(mediaSetNameResult.Value,
                                                              videos.VideoFiles,
                                                              supportedImageFiles,
                                                              masterfile ?? Maybe<Masterfile>.None,
                                                              filesWithEmptyTitleTags));
        }

        _logger.LogInformation("Gruppierung der Medien-Dateien in Mediensets erfolgreich.");
        _logger.LogInformation("Anzahl Mediensets: {Count}", mediaFilesByMediaSet.Count);

        foreach (var mediaFiles in mediaFilesByMediaSet)
        {
            _logger.LogInformation("Medienset: {Name}", mediaFiles.Name);
            _logger.LogInformation("Anzahl Videos: {Count}", mediaFiles.VideoFiles.Count());
            _logger.LogInformation("Anzahl Bilder: {Count}", mediaFiles.ImageFiles.Count());
            _logger.LogInformation("Anzahl Dateien ohne Titel-Tag: {Count}", mediaFiles.EmptyTitleTags.Count());
        }

        return Result.Success(mediaFilesByMediaSet);
    }

}

/// <summary>
/// Repräsentiert eine Gruppierung von unterstützten Videodateien nach Medienset.
/// </summary>
/// <param name="NameString"></param>
/// <param name="VideoFiles"></param>
/// <returns></returns>
internal record VideosByMediaSet(string NameString, IEnumerable<SupportedVideo> VideoFiles);

/// <summary>
/// Repräsentiert eine Gruppierung von separat gelisteten unterstützten Medien-Dateien nach Medienset.
/// </summary>
/// <param name="Title"></param>
/// <param name="VideoFiles"></param>
/// <param name="ImageFiles"></param>
/// <param name="EmptyTitleTags"></param>
/// <returns></returns>
public record MediaFilesByMediaSet(MediaSetName Name,
                                   IEnumerable<SupportedVideo> VideoFiles,
                                   IEnumerable<SupportedImage> ImageFiles,
                                   Maybe<Masterfile> Masterfile,
                                   IEnumerable<FileInfo> EmptyTitleTags);