using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Models;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public class Engine
{
    private readonly ModuleSettings _moduleSettings;
    private readonly ApplicationSettings _applicationSettings;
    private readonly ILogger<Engine> _logger;
    private readonly InfuseMetadataXmlService _infuseMetadataXmlService;
    private readonly MediaIntegratorService _mediaIntegratorService;
    private readonly IFileOperations _fileOperations;


    public Engine(IOptions<ModuleSettings> moduleSettings,
                  IOptions<ApplicationSettings> applicationSettings,
                  ILogger<Engine> logger,
                  InfuseMetadataXmlService infuseMetadataXmlService,
                  MediaIntegratorService mediaIntegratorService,
                  IFileOperations fileOperations)
    {
        _moduleSettings = moduleSettings.Value;
        _applicationSettings = applicationSettings.Value;
        _logger = logger;
        _infuseMetadataXmlService = infuseMetadataXmlService;
        _mediaIntegratorService = mediaIntegratorService;
        _fileOperations = fileOperations;
    }

    public async Task<List<LocalMediaServerFiles>> StartAsync(IProgress<string> progress, List<MediaSet> mediaSets)
    {
        _logger.LogInformation("InfuseMediaLibrary-Feature gestartet.");

        _logger.LogInformation("Iteriere über alle Mediensets und versuche, die Medien-Dateien in die Infuse-Mediathek zu integrieren.");
        var integratedMediaServerFilesByMediaSet = new List<LocalMediaServerFiles>();
        foreach (var mediaSet in mediaSets)
        {
            var integrateMediaSetResult = await _mediaIntegratorService.IntegrateMediaSetToInfuseMediaLibrary(mediaSet);
            if (integrateMediaSetResult.IsFailure)
            {
                _logger.LogWarning($"Fehler beim Integrieren des Mediensets {mediaSet.Title} in die Infuse-Mediathek: {integrateMediaSetResult.Error}");
                _logger.LogInformation("Das Medienset wird ignoriert.");
                continue;
            }

            if (integrateMediaSetResult.Value.HasNoValue)
            {
                _logger.LogInformation($"Das Medienset {mediaSet.Title} enthält keine Medien für die Integration in die Infuse-Mediathek.");
                _logger.LogInformation("Das Medienset wird ignoriert.");
                continue;
            }

            _logger.LogInformation($"Medienset {mediaSet.Title} wurde erfolgreich in die Infuse-Mediathek integriert.");
            integratedMediaServerFilesByMediaSet.Add(integrateMediaSetResult.Value.Value);
        }

        return integratedMediaServerFilesByMediaSet;
    }
}