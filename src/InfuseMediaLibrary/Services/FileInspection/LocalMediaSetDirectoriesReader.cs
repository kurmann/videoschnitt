using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;

/// <summary>
/// Verantwortlich für das Auslesen der Verzeichnisse, in denen die Mediensets gespeichert sind.
/// </summary>
internal class LocalMediaSetDirectoryReader
{
    private readonly ILogger<LocalMediaSetDirectoryReader> _logger;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;

    public LocalMediaSetDirectoryReader(ILogger<LocalMediaSetDirectoryReader> logger, IOptions<MediaSetOrganizerSettings> mediaSetOrganizerSettings)
    {
        _logger = logger;
        _mediaSetOrganizerSettings = mediaSetOrganizerSettings.Value;
    }

    public Result<List<MediaSetDirectory>> GetMediaSetDirectories(string directoryPath)
    {
        try
        {
            var directoryInfo = new DirectoryInfo(directoryPath);
            return GetMediaSetDirectories(directoryInfo);
        }
        catch
        {
            return Result.Failure<List<MediaSetDirectory>>($"Das Verzeichnis '{directoryPath}' konnte nicht geöffnet werden.");
        }
    }

    public Result<List<MediaSetDirectory>> GetMediaSetDirectories(DirectoryInfo directoryInfo)
    {
        try
        {
            // Prüfen, ob das Verzeichnis existiert
            if (!directoryInfo.Exists)
            {
                return Result.Failure<List<MediaSetDirectory>>($"Das Verzeichnis '{directoryInfo.FullName}' existiert nicht.");
            }

            // Ermittle alle Unterverzeichnisse des Quellverzeichnisses
            var mediaSetDirectories = directoryInfo.GetDirectories();
            if (mediaSetDirectories.Length == 0)
            {
                _logger.LogInformation("Keine Mediensets im Verzeichnis {Directory} gefunden.", directoryInfo.FullName);
                return Result.Success(new List<MediaSetDirectory>());
            }

            var localMediaSetDirectories = new List<MediaSetDirectory>();
            foreach (var mediaSetDirectory in mediaSetDirectories)
            {
                // Parse den Namen des Mediensets
                var mediaSetNameResult = MediaSetName.Create(mediaSetDirectory.Name);
                if (mediaSetNameResult.IsFailure)
                {
                    _logger.LogWarning("Fehler beim Parsen des Medienset-Namens '{MediaSetName}': {Error}", mediaSetDirectory.Name, mediaSetNameResult.Error);
                    _logger.LogWarning("Das Verzeichnis '{MediaSetDirectory}' wird ignoriert.", mediaSetDirectory.FullName);
                    continue;
                }
                var mediaSetName = mediaSetNameResult.Value;

                // Suche nach dem Unterverzeichnis, das die Dateien für den Medienserver enthält
                var mediaServerFilesDirectory = mediaSetDirectory.GetDirectories()
                    .FirstOrDefault(d => d.Name == _mediaSetOrganizerSettings.MediaSet.MediaServerFilesSubDirectoryName);

                // Suche nach dem Unterverzeichnis, das die Artwork-Bilder für das Medienset enthält
                var artworkDirectoryInfo = mediaSetDirectory.GetDirectories()
                    .FirstOrDefault(d => d.Name == _mediaSetOrganizerSettings.MediaSet.ImageFilesSubDirectoryName);
                var maybeArtworkDirectory = artworkDirectoryInfo != null ? new ArtworkDirectory(artworkDirectoryInfo, mediaSetName) : Maybe<ArtworkDirectory>.None;
                var maybeMediaServerFilesDirectory = mediaServerFilesDirectory != null ? new MediaServerFilesDirectory(mediaServerFilesDirectory, mediaSetName) : Maybe<MediaServerFilesDirectory>.None;
    
                var localMediaSetDirectory = new MediaSetDirectory(mediaSetDirectory, mediaSetNameResult.Value, maybeArtworkDirectory, maybeMediaServerFilesDirectory);
                localMediaSetDirectories.Add(localMediaSetDirectory);
            }

            return Result.Success(localMediaSetDirectories);
        }
        catch
        {
            return Result.Failure<List<MediaSetDirectory>>($"Das Verzeichnis '{directoryInfo.FullName}' konnte nicht geöffnet werden.");
        }
    
    }
}

/// <summary>
/// Definiert ein Verzeichnis, in dem sich die Dateien für ein Medienset befinden.
/// </summary>
/// <param name="DirectoryInfo"></param>
/// <param name="MediaSetName"></param>
/// <param name="ArtworkDirectory"></param>
/// <param name="MediaServerFilesDirectory"></param>
/// <returns></returns>
internal record MediaSetDirectory(DirectoryInfo DirectoryInfo, MediaSetName MediaSetName, Maybe<ArtworkDirectory> ArtworkDirectory, Maybe<MediaServerFilesDirectory> MediaServerFilesDirectory)
{
    public string Name => MediaSetName;

    public override string ToString()
    {
        return MediaSetName;
    }

    public static implicit operator DirectoryInfo(MediaSetDirectory localMediaSetDirectory)
    {
        return localMediaSetDirectory.DirectoryInfo;
    }
}

/// <summary>
/// Definiert ein Verzeichnis, in dem sich die Artwork-Bilder für ein Medienset befinden.
/// </summary>
/// <param name="DirectoryInfo"></param>
/// <param name="MediaSetName"></param>
/// <returns></returns>
internal record ArtworkDirectory(DirectoryInfo DirectoryInfo, MediaSetName MediaSetName)
{
    public override string ToString()
    {
        return MediaSetName;
    }

    public static implicit operator DirectoryInfo(ArtworkDirectory artworkDirectory)
    {
        return artworkDirectory.DirectoryInfo;
    }

    public Result<List<SupportedImage>> GetSupportedImages() => SupportedImage.GetSupportedImagesFromDirectory(DirectoryInfo);
}

internal record MediaServerFilesDirectory(DirectoryInfo DirectoryInfo, MediaSetName MediaSetName)
{
    public override string ToString()
    {
        return MediaSetName;
    }

    public static implicit operator DirectoryInfo(MediaServerFilesDirectory mediaServerFilesDirectory)
    {
        return mediaServerFilesDirectory.DirectoryInfo;
    }

    public Result<List<SupportedVideo>> GetSupportedVideos() => SupportedVideo.GetSupportedVideosFromDirectory(DirectoryInfo);
}