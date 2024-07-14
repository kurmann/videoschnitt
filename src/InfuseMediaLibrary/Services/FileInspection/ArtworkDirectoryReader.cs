using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;

/// <summary>
/// Verantwortlich für das Auslesen des Verzeichnisses, in dem die Artwork-Bilder für ein Medienset gespeichert sind.
/// </summary>
internal class ArtworkDirectoryReader
{
    public readonly ILogger<ArtworkDirectoryReader> _logger;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;

    public ArtworkDirectoryReader(ILogger<ArtworkDirectoryReader> logger, IOptions<MediaSetOrganizerSettings> mediaSetOrganizerSettings)
    {
        _logger = logger;
        _mediaSetOrganizerSettings = mediaSetOrganizerSettings.Value;
    }

    public Result<ArtworkDirectoryContent> GetDirectoryContent(string directoryPath)
    {
        try
        {
            var directoryInfo = new DirectoryInfo(directoryPath);
            return GetDirectoryContent(directoryInfo);
        }
        catch
        {
            return Result.Failure<ArtworkDirectoryContent>($"Das Verzeichnis '{directoryPath}' konnte nicht geöffnet werden.");
        }
    }

    public Result<ArtworkDirectoryContent> GetDirectoryContent(DirectoryInfo directoryInfo)
    {
        try
        {
            // Prüfen, ob das Verzeichnis existiert
            if (!directoryInfo.Exists)
            {
                return Result.Failure<ArtworkDirectoryContent>($"Das Verzeichnis '{directoryInfo.FullName}' existiert nicht.");
            }

            // Ermittle die Suffixes für die Bilder
            var suffixForPortraitImages = _mediaSetOrganizerSettings.MediaSet.OrientationSuffixes.Portrait;
            var suffixForLandscapeImages = _mediaSetOrganizerSettings.MediaSet.OrientationSuffixes.Landscape;

            // Suche nach Bilder in Verzeichnis die mit den Suffixes enden (eins für Portrait und eins für Landscape) und berücksichtige nur JPG-Dateien
            var portraitImage = directoryInfo.GetFiles().FirstOrDefault(f => f.Name.EndsWith(suffixForPortraitImages, StringComparison.OrdinalIgnoreCase));
            var landscapeImage = directoryInfo.GetFiles().FirstOrDefault(f => f.Name.EndsWith(suffixForLandscapeImages, StringComparison.OrdinalIgnoreCase));

            return Result.Success(new ArtworkDirectoryContent(
                portraitImage != null ? Maybe<FileInfo>.From(portraitImage) : Maybe<FileInfo>.None,
                landscapeImage != null ? Maybe<FileInfo>.From(landscapeImage) : Maybe<FileInfo>.None));

        }
        catch
        {
            return Result.Failure<ArtworkDirectoryContent>($"Das Verzeichnis '{directoryInfo.FullName}' konnte nicht geöffnet werden.");
        }
    }
}

public record ArtworkDirectoryContent(Maybe<FileInfo> PortraitImage, Maybe<FileInfo> LandscapeImage);
