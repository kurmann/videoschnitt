using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.LocalIntegration;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.RemoteIntegration;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public interface IWorkflow
{
    const string WorkflowName = "InfuseMediaLibrary";

    Task<Result> ExecuteAsync();
}

internal class Workflow : IWorkflow
{
    private readonly ILogger<Workflow> _logger;
    private readonly LocalMediaSetDirectoryReader _localMediaSetDirectoryReader;
    private readonly MediaSetIntegrator _mediaSetIntegrator;
    private readonly InfuseMediaIntegrator _infuseMediaIntegrator;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;
    private readonly InfuseMediaLibrarySettings _infuseMediaLibrarySettings;

    public Workflow(ILogger<Workflow> logger, IOptions<MediaSetOrganizerSettings> mediaSetOrganizerSettings,
        IOptions<InfuseMediaLibrarySettings> infuseMediaLibrarySettings,
        LocalMediaSetDirectoryReader localMediaSetDirectoryReader, MediaSetIntegrator mediaSetIntegrator, InfuseMediaIntegrator infuseMediaIntegrator)
    {
        _logger = logger;
        _infuseMediaLibrarySettings = infuseMediaLibrarySettings.Value;
        _localMediaSetDirectoryReader = localMediaSetDirectoryReader;
        _mediaSetIntegrator = mediaSetIntegrator;
        _mediaSetOrganizerSettings = mediaSetOrganizerSettings.Value;
        _infuseMediaIntegrator = infuseMediaIntegrator;
    }

    public async Task<Result> ExecuteAsync()
    {
        var localIntegrationResult = await ExecuteLocalIntegration();
        if (localIntegrationResult.IsFailure)
        {
            return localIntegrationResult;
        }
        var remoteIntegrationResult = await ExecuteRemoteIntegration(localIntegrationResult.Value);
        if (remoteIntegrationResult.IsFailure)
        {
            return remoteIntegrationResult;
        }

        _logger.LogInformation("InfuseMediaLibrary-Workflow wurde erfolgreich ausgeführt.");
        return Result.Success();
    }

    private async Task<Result<List<IntegratedLocalInfuseMediaSet>>> ExecuteLocalIntegration()
    {
        var sourceDirectoryPath = _mediaSetOrganizerSettings.MediaSetPathLocal;
        var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);

        // Prüfe ob das Verzeichnis exisitiert
        if (sourceDirectory.Exists == false)
        {
            return Result.Failure<List<IntegratedLocalInfuseMediaSet>>($"Das Quellverzeichnis {sourceDirectoryPath} existiert nicht.");
        }

        var mediaSetDirectoriesResult = _localMediaSetDirectoryReader.GetMediaSetDirectories(sourceDirectory);
        if (mediaSetDirectoriesResult.IsFailure)
        {
            return Result.Failure<List<IntegratedLocalInfuseMediaSet>>($"Fehler beim Ermitteln der Medienset-Verzeichnisse: {mediaSetDirectoriesResult.Error}");
        }
        var mediaSetDirectories = mediaSetDirectoriesResult.Value;

        // Logge die Verzeichnisse, die gefunden wurden
        _logger.LogInformation("Folgende Medienset-Verzeichnisse wurden gefunden:");
        foreach (var mediaSetDirectory in mediaSetDirectories)
        {
            _logger.LogInformation("{MediaSetDirectory}", mediaSetDirectory.Name);
        }

        // Ziel ist es, die unterstützten Video- und Bildformate in die Infuse-Mediathek zu integrieren
        var integratedLocalInfuseMediaSets = new List<IntegratedLocalInfuseMediaSet>();
        foreach (var mediaSetDirectory in mediaSetDirectories)
        {
            var mediaSetIntegratorResult = await _mediaSetIntegrator.IntegrateMediaSetAsync(mediaSetDirectory);
            if (mediaSetIntegratorResult.IsFailure)
            {
                _logger.LogError("Fehler beim Integrieren des Mediensets {MediaSet}: {Error}", mediaSetDirectory.Name, mediaSetIntegratorResult.Error);
                _logger.LogInformation("Das Medienset {MediaSet} wird ignoriert.", mediaSetDirectory.Name);
                continue;
            }
            if (mediaSetIntegratorResult.Value.HasNoValue)
            {
                _logger.LogTrace("Das Medienset {MediaSet} wurde nicht integriert, da keine unterstützten Dateien gefunden wurden.", mediaSetDirectory.Name);
                continue;
            }
            integratedLocalInfuseMediaSets.Add(mediaSetIntegratorResult.Value.Value);
        }

        return Result.Success(integratedLocalInfuseMediaSets);
    }

    private async Task<Result> ExecuteRemoteIntegration(List<IntegratedLocalInfuseMediaSet> integratedLocalInfuseMediaSets)
    {
        _logger.LogWarning("Remote-Integration wird derzeit nicht unterstützt.");

        // Prüfe ob das Infuse-Mediathek-Verzeichnis auf dem Netzlaufwerk existiert
        if (!Directory.Exists(_infuseMediaLibrarySettings.InfuseMediaLibraryPathRemote))
        {
            // Wenn das Netzwerklaufwerk nicht verfügbar ist, wird die Remote-Integration nicht durchgeführt und mit einer Warnung beendet
            _logger.LogWarning("Das Infuse-Mediathek-Verzeichnis {InfuseMediaLibraryPathRemote} existiert nicht. Remote-Integration wird nicht durchgeführt.", _infuseMediaLibrarySettings.InfuseMediaLibraryPathRemote);
            return Result.Success();
        }

        // Iteriere über die integrierten lokalen Infuse-Medien und führe die Remote-Integration durch
        foreach (var integratedLocalInfuseMediaSet in integratedLocalInfuseMediaSets)
        {
            // Null-Werte werden hier nicht erwartet, ignorieren wir sie
            if (integratedLocalInfuseMediaSet == null)
            {
                continue;
            }

            var remoteIntegrationResult = await _infuseMediaIntegrator.IntegrateInfuseMediaAsync(integratedLocalInfuseMediaSet);
            if (remoteIntegrationResult.IsFailure)
            {
                _logger.LogError("Fehler bei der Remote-Integration des Mediensets {MediaSet}: {Error}", integratedLocalInfuseMediaSet, remoteIntegrationResult.Error);
                continue;
            }
        }

        return Result.Success();
    }
}