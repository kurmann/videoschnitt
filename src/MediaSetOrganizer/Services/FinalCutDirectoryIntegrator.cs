using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Kurmann.Videoschnitt.Common.Models;

namespace Kurmann.Videoschnitt.MediaSetOrganizer.Services;

/// <summary>
/// Verantwortlich für die Integration von Dateien aus dem Final Cut Export-Verzeichnis.
/// </summary>
public class FinalCutDirectoryIntegrator
{
    private readonly ILogger<FinalCutDirectoryIntegrator> _logger;
    private readonly IFileOperations _fileOperations;
    private readonly InputDirectoryReaderService _inputDirectoryReaderService;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;

    public FinalCutDirectoryIntegrator(
        ILogger<FinalCutDirectoryIntegrator> logger,
        InputDirectoryReaderService inputDirectoryReaderService,
        IOptions<MediaSetOrganizerSettings> mediaSetOrganizerSettings,
        IFileOperations fileOperations)
    {
        _logger = logger;
        _inputDirectoryReaderService = inputDirectoryReaderService;
        _mediaSetOrganizerSettings = mediaSetOrganizerSettings.Value;
        _fileOperations = fileOperations;
    }

    public async Task<Result<IntegratedFinalCutExportFiles>> IntegrateFinalCutExportFilesAsync()
    {
        _logger.LogInformation("Integriere Dateien aus dem Final Cut Export-Verzeichnis {finalCutDir} in das Eingangsverzeichnis {inputdirectory}.", _mediaSetOrganizerSettings.FinalCutExportDirectory, _mediaSetOrganizerSettings.InputDirectory);

        var inputDirectoryContent = await _inputDirectoryReaderService.ReadInputDirectoryAsync(_mediaSetOrganizerSettings.FinalCutExportDirectory);
        if (inputDirectoryContent.IsFailure)
        {
            return Result.Failure<IntegratedFinalCutExportFiles>($"Fehler beim Lesen des Final Cut Export-Verzeichnisses: {inputDirectoryContent.Error}");
        }

        if (inputDirectoryContent.Value.IsEmpty)
        {
            _logger.LogInformation("Keine Dateien im Final Cut Export-Verzeichnis gefunden.");
            return Result.Success(new IntegratedFinalCutExportFiles());
        }
        if (!inputDirectoryContent.Value.HasAnySupportedFiles)
        {
            _logger.LogInformation("Keine unterstützten Dateien im Final Cut Export-Verzeichnis gefunden.");
            _logger.LogInformation("Dies kann daran liegen, dass alle Dateien bereits im Eingangsverzeichnis vorhanden sind oder gerade Dateien exportiert werden.");
            return Result.Success(new IntegratedFinalCutExportFiles());
        }

        _logger.LogInformation("Anzahl der unterstützten Videos: {count}", inputDirectoryContent.Value.SupportedVideos.Count);
        _logger.LogInformation("Anzahl der unterstützten Bilder: {count}", inputDirectoryContent.Value.SupportedImages.Count);
        
        var supportedVideos = new List<SupportedVideo>();
        if (inputDirectoryContent.Value.HasSupportedVideos)
        {
            _logger.LogInformation("Verschiebe unterstützte Videos aus dem Final Cut Export-Verzeichnis nach {inputdirectory}", _mediaSetOrganizerSettings.InputDirectory);
            foreach (var video in inputDirectoryContent.Value.SupportedVideos)
            {
                var targetPath = Path.Combine(_mediaSetOrganizerSettings.InputDirectory, video.FileInfo.Name);
                var result = await _fileOperations.MoveFileAsync(video.FileInfo.FullName, targetPath);
                if (result.IsFailure)
                {
                    return Result.Failure<IntegratedFinalCutExportFiles>($"Fehler beim Verschieben der Datei {video.FileInfo.FullName} nach {targetPath}: {result.Error}");
                }
                supportedVideos.Add(video);
            }
        }

        // Hinweis: Masterdateien werden nicht zu den unterstützten Videodateien gezählt, da sie nicht an den Medienserver oder über das Internet gestreamt werden.
        var masterfiles = new List<Masterfile>();
        if (inputDirectoryContent.Value.HasMasterfiles)
        {
            _logger.LogInformation("Verschiebe Masterdateien aus dem Final Cut Export-Verzeichnis nach {inputdirectory}", _mediaSetOrganizerSettings.InputDirectory);
            foreach (var masterfile in inputDirectoryContent.Value.Masterfiles)
            {
                var targetPath = Path.Combine(_mediaSetOrganizerSettings.InputDirectory, masterfile.FileInfo.Name);
                var result = await _fileOperations.MoveFileAsync(masterfile.FileInfo.FullName, targetPath);
                if (result.IsFailure)
                {
                    return Result.Failure<IntegratedFinalCutExportFiles>($"Fehler beim Verschieben der Datei {masterfile.FileInfo.FullName} nach {targetPath}: {result.Error}");
                }
                masterfiles.Add(masterfile);
            }
        }

        var supportedImages = new List<SupportedImage>();
        if (inputDirectoryContent.Value.HasSupportedImages)
        {
            _logger.LogInformation("Verschiebe unterstützte Bilder aus dem Final Cut Export-Verzeichnis nach {inputdirectory}", _mediaSetOrganizerSettings.InputDirectory);
            foreach (var image in inputDirectoryContent.Value.SupportedImages)
            {
                var targetPath = Path.Combine(_mediaSetOrganizerSettings.InputDirectory, image.FileInfo.Name);
                var result = await _fileOperations.MoveFileAsync(image.FileInfo.FullName, targetPath);
                if (result.IsFailure)
                {
                    return Result.Failure<IntegratedFinalCutExportFiles>($"Fehler beim Verschieben der Datei {image.FileInfo.FullName} nach {targetPath}: {result.Error}");
                }
                supportedImages.Add(image);
            }
        }

        _logger.LogInformation("Dateien aus dem Final Cut Export-Verzeichnis erfolgreich integriert.");

        return new IntegratedFinalCutExportFiles
        {
            Videos = supportedVideos,
            Images = supportedImages,
            Masterfiles = masterfiles
        };
    }
}

public record IntegratedFinalCutExportFiles
{
    public List<SupportedVideo> Videos { get; init; } = new();
    public List<SupportedImage> Images { get; init; } = new();
    public List<Masterfile> Masterfiles { get; init; } = new();
}
