using CSharpFunctionalExtensions;
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

    public Result<List<LocalMediaSetDirectory>> GetMediaSetDirectories(string directoryPath)
    {
        try
        {
            var directoryInfo = new DirectoryInfo(directoryPath);
            return GetMediaSetDirectories(directoryInfo);
        }
        catch
        {
            return Result.Failure<List<LocalMediaSetDirectory>>($"Das Verzeichnis '{directoryPath}' konnte nicht geöffnet werden.");
        }
    }

    public Result<List<LocalMediaSetDirectory>> GetMediaSetDirectories(DirectoryInfo directoryInfo)
    {
        try
        {
            // Prüfen, ob das Verzeichnis existiert
            if (!directoryInfo.Exists)
            {
                return Result.Failure<List<LocalMediaSetDirectory>>($"Das Verzeichnis '{directoryInfo.FullName}' existiert nicht.");
            }

            // Ermittle alle Unterverzeichnisse des Quellverzeichnisses
            var mediaSetDirectories = directoryInfo.GetDirectories();
            if (mediaSetDirectories.Length == 0)
            {
                _logger.LogInformation("Keine Mediensets im Verzeichnis {Directory} gefunden.", directoryInfo.FullName);
                return Result.Success(new List<LocalMediaSetDirectory>());
            }

            var localMediaSetDirectories = new List<LocalMediaSetDirectory>();
            foreach (var mediaSetDirectory in mediaSetDirectories)
            {
                // Suche nach dem Unterverzeichnis, das die Dateien für den Medienserver enthält
                var mediaServerFilesDirectory = mediaSetDirectory.GetDirectories()
                    .FirstOrDefault(d => d.Name == _mediaSetOrganizerSettings.MediaSet.MediaServerFilesSubDirectoryName);

                // Suche nach dem Unterverzeichnis, das die Artwork-Bilder für das Medienset enthält
                var artworkDirectory = mediaSetDirectory.GetDirectories()
                    .FirstOrDefault(d => d.Name == _mediaSetOrganizerSettings.MediaSet.ImageFilesSubDirectoryName);

                // Parse media set name
                var mediaSetNameResult = MediaSetTitle.Create(mediaSetDirectory.Name);
                if (mediaSetNameResult.IsFailure)
                {
                    _logger.LogError("Fehler beim Parsen des Medienset-Namens '{MediaSetName}': {Error}", mediaSetDirectory.Name, mediaSetNameResult.Error);
                    continue;
                }

                var localMediaSetDirectory = new LocalMediaSetDirectory(mediaSetDirectory, mediaSetNameResult.Value, artworkDirectory ?? Maybe<DirectoryInfo>.None, mediaServerFilesDirectory ?? Maybe<DirectoryInfo>.None);
                localMediaSetDirectories.Add(localMediaSetDirectory);
            }

            return Result.Success(localMediaSetDirectories);
        }
        catch
        {
            return Result.Failure<List<LocalMediaSetDirectory>>($"Das Verzeichnis '{directoryInfo.FullName}' konnte nicht geöffnet werden.");
        }
    
    }
}

internal record LocalMediaSetDirectory(DirectoryInfo DirectoryInfo, MediaSetTitle MediaSetName, Maybe<DirectoryInfo> ArtworkDirectory, Maybe<DirectoryInfo> MediaServerFilesDirectory)
{
    public override string ToString()
    {
        return MediaSetName;
    }

    public static implicit operator DirectoryInfo(LocalMediaSetDirectory localMediaSetDirectory)
    {
        return localMediaSetDirectory.DirectoryInfo;
    }
}
