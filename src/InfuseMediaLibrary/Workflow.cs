using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.Integration;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public interface IWorkflow
{
    const string WorkflowName = "InfuseMediaLibrary";

    Task<Result> ExecuteAsync();
}

internal class Workflow : IWorkflow
{
    private readonly ApplicationSettings _applicationSettings;
    private readonly ILogger<Workflow> _logger;
    private readonly LocalMediaSetDirectoryReader _localMediaSetDirectoryReader;
    private readonly MediaSetIntegrator _mediaSetIntegrator;

    public Workflow(IOptions<ApplicationSettings> applicationSettings, ILogger<Workflow> logger, 
        LocalMediaSetDirectoryReader localMediaSetDirectoryReader, MediaSetIntegrator mediaSetIntegrator)
    {
        _applicationSettings = applicationSettings.Value;
        _logger = logger;
        _localMediaSetDirectoryReader = localMediaSetDirectoryReader;
        _mediaSetIntegrator = mediaSetIntegrator;
    }

    public async Task<Result> ExecuteAsync()
    {
        var sourceDirectoryPath = _applicationSettings.MediaSetPathLocal;

        // todo: Hardcodierter Verzeichnispfad, später aus Konfiguration auslesen
        sourceDirectoryPath = "/Users/patrickkurmann/Movies/Mediensets";

        var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);

        // Prüfe ob das Verzeichnis exisitiert
        if (sourceDirectory.Exists == false)
        {
            return Result.Failure($"Das Verzeichnis {sourceDirectory} existiert nicht.");
        }

        var mediaSetDirectoriesResult = _localMediaSetDirectoryReader.GetMediaSetDirectories(sourceDirectory);
        if (mediaSetDirectoriesResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Ermitteln der Medienset-Verzeichnisse im Quellverzeichnis {sourceDirectoryPath}. Fehler: {mediaSetDirectoriesResult.Error}");
        }
        var mediaSetDirectories = mediaSetDirectoriesResult.Value;

        // Logge die Verzeichnisse, die gefunden wurden
        _logger.LogInformation("Folgende Medienset-Verzeichnisse wurden gefunden:");
        foreach (var mediaSetDirectory in mediaSetDirectories)
        {
            _logger.LogInformation("{MediaSetDirectory}", mediaSetDirectory.Name);
        }

        // Ziel ist es, die unterstützten Video- und Bildformate in die Infuse-Mediathek zu integrieren
        foreach (var mediaSetDirectory in mediaSetDirectories)
        {
            var mediaSetIntegratorResult = await _mediaSetIntegrator.IntegrateMediaSetAsync(mediaSetDirectory);
            if (mediaSetIntegratorResult.IsFailure)
            {
                _logger.LogError("Fehler beim Integrieren des Mediensets {MediaSet}: {Error}", mediaSetDirectory.Name, mediaSetIntegratorResult.Error);
                _logger.LogInformation("Das Medienset {MediaSet} wird ignoriert.", mediaSetDirectory.Name);
                continue;
            }
        }

        _logger.LogInformation("InfuseMediaLibrary-Workflow wurde erfolgreich ausgeführt.");
        return Result.Success();
    }
}