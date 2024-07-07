using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services;
using Kurmann.Videoschnitt.Common.Models;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public class Engine
{
    private readonly ApplicationSettings _applicationSettings;
    private readonly ILogger<Engine> _logger;
    private readonly MediaIntegratorService _mediaIntegratorService;


    public Engine(IOptions<ApplicationSettings> applicationSettings, ILogger<Engine> logger, MediaIntegratorService mediaIntegratorService)
    {
        _applicationSettings = applicationSettings.Value;
        _logger = logger;
        _mediaIntegratorService = mediaIntegratorService;
    }

    public async Task<Result<List<LocalMediaServerFiles>>> StartAsync(List<MediaSet> mediaSets)
    {
        _logger.LogInformation("InfuseMediaLibrary-Feature gestartet.");
        LogRelevantConfiguration();

        _logger.LogInformation("Iteriere über alle Mediensets und versuche, die Medien-Dateien in die Infuse-Mediathek zu integrieren.");
        var integratedMediaServerFilesByMediaSet = new List<LocalMediaServerFiles>();
        foreach (var mediaSet in mediaSets)
        {
            var integrateMediaSetResult = await _mediaIntegratorService.IntegrateMediaSetToInfuseMediaLibrary(mediaSet);
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

    private Result LogRelevantConfiguration()
    {
        // Prüfe ob das Infuse-Mediathek-Verzeichnis konfiguriert ist
        if (_applicationSettings.IsDefaultInfuseMediaLibraryPath)
        {
            _logger.LogInformation("Das Infuse-Mediathek-Verzeichnis wurde nicht konfiguriert, verwende den Standardpfad {path}", _applicationSettings.InfuseMediaLibraryPath);
        }
        else if (_applicationSettings.InfuseMediaLibraryPath == null)
        {
            return Result.Failure("Das Infuse-Mediathek-Verzeichnis wurde nicht korrekt aus den Einstellungen geladen.");
        }
        else 
        {
            _logger.LogInformation("Das Infuse-Mediathek-Verzeichnis wurde aus den Einstellungen geladen: {path}", _applicationSettings.InfuseMediaLibraryPath);
        }

        // Prüfe das Eingangsverzeichnis
        if (_applicationSettings.IsDefaultInputDirectory)
        {
            _logger.LogInformation("Das Eingangsverzeichnis wurde nicht konfiguriert, verwende den Standardpfad {path}", _applicationSettings.InputDirectory);
        }
        else if (_applicationSettings.InputDirectory == null)
        {
            return Result.Failure("Das Eingangsverzeichnis wurde nicht korrekt aus den Einstellungen geladen.");
        }
        else
        {
            _logger.LogInformation("Das Eingangsverzeichnis wurde aus den Einstellungen geladen: {path}", _applicationSettings.InputDirectory);
        }

        return Result.Success();
    }
}