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

    public FinalCutDirectoryIntegrator(
        ILogger<FinalCutDirectoryIntegrator> logger,
        IOptions<ApplicationSettings> applicationSettings,
        IFileOperations fileOperations)
    {
        _logger = logger;
        _applicationSettings = applicationSettings.Value;
        _fileOperations = fileOperations;
    }

    public async Task<Result<IntegratedFinalCutExportFiles>> IntegrateFinalCutExportFilesAsync()
    {
        var inputDirectory = _applicationSettings.InputDirectory;
        _logger.LogInformation("Integriere Dateien aus dem Final Cut Export-Verzeichnis {path} nach {inputdirectory}", _applicationSettings.FinalCutExportDirectory, inputDirectory);

        return new IntegratedFinalCutExportFiles();
    }
}

public record IntegratedFinalCutExportFiles
{
    public List<SupportedVideo> Videos { get; init; } = new();
    public List<SupportedImage> Images { get; init; } = new();
}
