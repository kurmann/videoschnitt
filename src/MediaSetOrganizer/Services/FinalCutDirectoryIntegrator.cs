using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;

namespace Kurmann.Videoschnitt.MediaSetOrganizer.Services;

/// <summary>
/// Verantwortlich für die Integration von Dateien aus dem Final Cut Export-Verzeichnis.
/// </summary>
public class FinalCutDirectoryIntegrator
{
    private readonly ILogger<FinalCutDirectoryIntegrator> _logger;
    private readonly ApplicationSettings _applicationSettings;
    private readonly IFileOperations _fileOperations;
    private readonly InputDirectoryReaderService _inputDirectoryReaderService;

    public FinalCutDirectoryIntegrator(
        ILogger<FinalCutDirectoryIntegrator> logger,
        IOptions<ApplicationSettings> applicationSettings,
        InputDirectoryReaderService inputDirectoryReaderService,
        IFileOperations fileOperations)
    {
        _logger = logger;
        _applicationSettings = applicationSettings.Value;
        _inputDirectoryReaderService = inputDirectoryReaderService;
        _fileOperations = fileOperations;
    }

    public async Task<Result<IntegratedFinalCutExportFiles>> IntegrateFinalCutExportFilesAsync()
    {
        var inputDirectory = _applicationSettings.InputDirectory;
        _logger.LogInformation("Integriere Dateien aus dem Final Cut Export-Verzeichnis {path} nach {inputdirectory}", _applicationSettings.FinalCutExportDirectory, inputDirectory);

        _logger.LogInformation("Lese Dateien aus dem Final Cut Export-Verzeichnis.");
        var inputDirectoryContent = await _inputDirectoryReaderService.ReadInputDirectoryAsync(_applicationSettings.FinalCutExportDirectory);
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
        
        if (inputDirectoryContent.Value.HasSupportedVideos)
        {
            _logger.LogInformation("Verschiebe unterstützte Videos aus dem Final Cut Export-Verzeichnis nach {inputdirectory}", inputDirectory);
            foreach (var video in inputDirectoryContent.Value.SupportedVideos)
            {
                var targetPath = Path.Combine(inputDirectory, video.FileInfo.Name);
                var result = await _fileOperations.MoveFileAsync(video.FileInfo.FullName, targetPath);
                if (result.IsFailure)
                {
                    return Result.Failure<IntegratedFinalCutExportFiles>($"Fehler beim Verschieben der Datei {video.FileInfo.FullName} nach {targetPath}: {result.Error}");
                }
            }
        }

        if (inputDirectoryContent.Value.HasSupportedImages)
        {
            _logger.LogInformation("Verschiebe unterstützte Bilder aus dem Final Cut Export-Verzeichnis nach {inputdirectory}", inputDirectory);
            foreach (var image in inputDirectoryContent.Value.SupportedImages)
            {
                var targetPath = Path.Combine(inputDirectory, image.FileInfo.Name);
                var result = await _fileOperations.MoveFileAsync(image.FileInfo.FullName, targetPath);
                if (result.IsFailure)
                {
                    return Result.Failure<IntegratedFinalCutExportFiles>($"Fehler beim Verschieben der Datei {image.FileInfo.FullName} nach {targetPath}: {result.Error}");
                }
            }
        }

        _logger.LogInformation("Dateien aus dem Final Cut Export-Verzeichnis erfolgreich integriert.");


        return new IntegratedFinalCutExportFiles();
    }
}

public record IntegratedFinalCutExportFiles
{
    public List<SupportedVideo> Videos { get; init; } = new();
    public List<SupportedImage> Images { get; init; } = new();
}
