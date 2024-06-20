using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

/// <summary>
/// Verantwortlich um Mediensets in Unterverzeichnisse zu organisieren.
/// </summary>
public class MediaSetSubDirectoryOrganizer
{
    private readonly ILogger<MediaSetSubDirectoryOrganizer> _logger;

    public MediaSetSubDirectoryOrganizer(ILogger<MediaSetSubDirectoryOrganizer> logger)
    {
        _logger = logger;
    }

    public Result<List<MediaSetDirectory>> MoveMediaSetsToDirectories(IEnumerable<MediaFilesByMediaSet> mediaSets, string inputDirectory)
    {
        var mediaSetDirectories = new List<MediaSetDirectory>();
        foreach (var mediaSet in mediaSets)
        {
            var mediaSetDirectory = Path.Combine(inputDirectory, mediaSet.Title);
            if (!Directory.Exists(mediaSetDirectory))
            {
                Directory.CreateDirectory(mediaSetDirectory);
            }

            var mediaFiles = mediaSet.ImageFiles.Select(item => item.FileInfo).Concat(mediaSet.VideoFiles.Select(item => item.FileInfo));
            foreach (var mediaFile in mediaFiles)
            {
                var destination = Path.Combine(mediaSetDirectory, mediaFile.Name);
                try
                {
                    File.Move(mediaFile.FullName, destination);
                }
                catch (Exception ex)
                {
                    return Result.Failure<List<MediaSetDirectory>>($"Fehler beim Verschieben der Datei '{mediaFile.FullName}' nach '{destination}': {ex.Message}");
                }

                _logger.LogInformation($"Datei '{mediaFile.FullName}' erfolgreich nach '{destination}' verschoben.");
            }

            mediaSetDirectories.Add(new MediaSetDirectory(new DirectoryInfo(mediaSetDirectory), mediaSet.Title, mediaSet.ImageFiles, mediaSet.VideoFiles));
        }

        return Result.Success(mediaSetDirectories);
    }

}

public record MediaSetDirectory(DirectoryInfo DirectoryInfo, string MediaSetTitle, IEnumerable<SupportedImage> ImageFiles, IEnumerable<SupportedVideo> VideoFiles);