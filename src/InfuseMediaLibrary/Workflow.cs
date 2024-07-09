using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services;
using Kurmann.Videoschnitt.Common.Models;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public class Workflow
{
    public const string WorkflowName = "LocalInfuseMediaLibraryIntegration";
    private readonly ApplicationSettings _applicationSettings;
    private readonly ILogger<Workflow> _logger;
    private readonly MediaIntegratorService _mediaIntegratorService;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;

    public Workflow(IOptions<ApplicationSettings> applicationSettings, IOptions<MediaSetOrganizerSettings> mediaSetOrganizerSettings,
        ILogger<Workflow> logger, MediaIntegratorService mediaIntegratorService)
    {
        _applicationSettings = applicationSettings.Value;
        _mediaSetOrganizerSettings = mediaSetOrganizerSettings.Value;
        _logger = logger;
        _mediaIntegratorService = mediaIntegratorService;
    }

    public async Task<Result> StartAsync()
    {
        var sourceDirectoryPath = _applicationSettings.MediaSetPathLocal;
        var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);

        // Prüfe ob das Verzeichnis exisitiert
        if (sourceDirectory.Exists == false)
        {
            return Result.Failure($"Das Verzeichnis {sourceDirectory} existiert nicht.");
        }

        // Alle Unterverzeichnisse des Quellverzeichnisses werden als eigenständige Mediensets betrachtet
        var mediaSetDirectories = sourceDirectory.GetDirectories();
        if (mediaSetDirectories.Length == 0)
        {
            _logger.LogInformation("Keine Mediensets im Verzeichnis {Directory} gefunden.", sourceDirectoryPath);
            return Result.Success();
        }
        var mediaSetNamesCommaseparated = string.Join(", ", mediaSetDirectories.Select(d => d.Name));
        _logger.LogInformation("Folgende Mediensets wurden im Verzeichnis {Directory} gefunden: {MediaSets}", sourceDirectoryPath, mediaSetNamesCommaseparated);

        return Result.Success();
    }

    public async Task<Result<List<LocalMediaServerFiles>>> StartAsync(List<MediaSet> mediaSets)
    {
        _logger.LogInformation("InfuseMediaLibrary-Feature gestartet.");

        if (_applicationSettings.InputDirectory == null)
        {
            return Result.Failure<List<LocalMediaServerFiles>>("Eingabeverzeichnis wurde nicht korrekt aus den Einstellungen geladen.");
        }

        _logger.LogInformation("Verzeichnis der lokalen Mediensets, das als Ausgangspunkt für die Integration in die Infuse-Mediathek dient: {Directory}", _applicationSettings.MediaSetPathLocal);

        _logger.LogInformation("Iteriere über alle Mediensets und versuche, die Medien-Dateien in die Infuse-Mediathek zu integrieren.");
        var integratedMediaServerFilesByMediaSet = new List<LocalMediaServerFiles>();
        foreach (var mediaSet in mediaSets)
        {
            var integrateMediaSetResult = await _mediaIntegratorService.IntegrateMediaSetToLocalInfuseMediaLibrary(mediaSet);
            if (integrateMediaSetResult.IsFailure)
            {
                _logger.LogWarning("Fehler beim Integrieren des Mediensets {Title} in die Infuse-Mediathek: {Error}", mediaSet.Title, integrateMediaSetResult.Error);
                _logger.LogInformation("Das Medienset wird ignoriert.");
                continue;
            }

            if (integrateMediaSetResult.Value.HasNoValue)
            {
                _logger.LogInformation("Das Medienset {Title} enthält keine Medien für die Integration in die Infuse-Mediathek.", mediaSet.Title);
                _logger.LogInformation("Das Medienset wird ignoriert.");
                continue;
            }

            _logger.LogInformation("Medienset {Title} wurde erfolgreich in die Infuse-Mediathek integriert.", mediaSet.Title);
            integratedMediaServerFilesByMediaSet.Add(integrateMediaSetResult.Value.Value);
        }

        return integratedMediaServerFilesByMediaSet;
    }
}