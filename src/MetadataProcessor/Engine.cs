using Kurmann.Videoschnitt.MetadataProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;
using Kurmann.Videoschnitt.MetadataProcessor.Entities;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor;

/// <summary>
/// Zentrale Steuereinheit für die Metadaten-Verarbeitung.
/// </summary>
public class Engine
{
    private readonly ModuleSettings _moduleSettings;
    private readonly ApplicationSettings _applicationSettings;
    private readonly FFmpegMetadataService _ffmpegMetadataService;
    private readonly ILogger<Engine> _logger;
    private readonly MediaSetService _mediaSetService;

    public Engine(ILogger<Engine> logger,
                  IOptions<ModuleSettings> moduleSettings,
                  IOptions<ApplicationSettings> applicationSettings,
                  FFmpegMetadataService ffmpegMetadataService,
                  MediaSetService mediaSetService)
    {
        _moduleSettings = moduleSettings.Value;
        _applicationSettings = applicationSettings.Value;
        _logger = logger;
        _ffmpegMetadataService = ffmpegMetadataService;
        _mediaSetService = mediaSetService;
    }

    public async Task<Result<List<MediaSetDirectory>>> Start(IProgress<string> progress)
    {
        progress.Report("Steuereinheit für die Metadaten-Verarbeitung gestartet.");

        // Prüfe ob die Einstellungen korrekt geladen wurden
        if (_applicationSettings.InputDirectory == null)
        {
            return Result.Failure<List<MediaSetDirectory>>("Eingabeverzeichnis wurde nicht korrekt aus den Einstellungen geladen.");
        }

        // Informiere über das Eingabeverzeichnis
        progress.Report($"Eingangsverzeichnis: {_applicationSettings.InputDirectory}");

        _logger.LogInformation("Versuche die Dateien im Eingangsverzeichnis in Medienset zu organisiseren.");
        var mediaSets = await _mediaSetService.GroupToMediaSets(_applicationSettings.InputDirectory);
        if (mediaSets.IsFailure)
        {
            return Result.Failure<List<MediaSetDirectory>>($"Fehler beim Gruppieren der Medien-Dateien in Mediensets: {mediaSets.Error}");
        }
        _logger.LogInformation("Mediensets erfolgreich gruppiert.");

        _logger.LogInformation("Verschiebe jedes Medienset in ein Unterverzeichnis mit dem Titel des Mediensets.");
        var mediaSetDirectories = new List<MediaSetDirectory>();
        foreach (var mediaSet in mediaSets.Value)
        {
            var mediaSetDirectory = Path.Combine(_applicationSettings.InputDirectory, mediaSet.Title);
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

    public record MediaSetDirectory(DirectoryInfo DirectoryInfo, string MediaSetTitle, IEnumerable<SupportedImage> ImageFiles, IEnumerable<SupportedVideo> VideoFiles);

}