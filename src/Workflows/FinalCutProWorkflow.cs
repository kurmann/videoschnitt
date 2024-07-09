using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Workflows.Abstractions;

namespace Kurmann.Videoschnitt.Workflows;

public class FinalCutProWorkflow : IAsyncWorkflow
{
    private readonly ILogger<FinalCutProWorkflow> _logger;
    private readonly MediaSetOrganizer.Engine _mediaSetOrganizerEngine;
    private readonly InfuseMediaLibrary.Workflow _infuseMediaLibraryEngine;

    public FinalCutProWorkflow(ILogger<FinalCutProWorkflow> logger, MediaSetOrganizer.Engine mediaSetOrganizerEngine, InfuseMediaLibrary.Workflow infuseMediaLibraryEngine)
    {
        _logger = logger;
        _mediaSetOrganizerEngine = mediaSetOrganizerEngine;
        _infuseMediaLibraryEngine = infuseMediaLibraryEngine;
    }

    public async Task<Result> ExecuteAsync()
    {
        _logger.LogInformation("Final Cut Pro Workflow gestartet.");

        _logger.LogInformation("Starte Medienset-Organisator");
        var mediaSetOrganizerResult = await _mediaSetOrganizerEngine.StartAsync();
        if (mediaSetOrganizerResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Ausführen des Final Cut Pro Workflows: {mediaSetOrganizerResult.Error}");
        }

        if (mediaSetOrganizerResult.Value.Count == 0)
        {
            _logger.LogInformation("Keine Medien-Dateien für die Integration in die Infuse-Mediathek gefunden.");
            return Result.Success();
        }

        _logger.LogInformation("Starte Integration in die Infuse-Mediathek");

        var integratedMediaServerFilesByMediaSet = await _infuseMediaLibraryEngine.StartAsync(mediaSetOrganizerResult.Value);
        if (integratedMediaServerFilesByMediaSet.IsFailure)
        {
            return Result.Failure($"Fehler beim Ausführen des Final Cut Pro Workflows: {integratedMediaServerFilesByMediaSet.Error}");
        }
        if (integratedMediaServerFilesByMediaSet.Value.Count == 0)
        {
            _logger.LogInformation("Keine Medien-Dateien für die Integration in die Infuse-Mediathek gefunden.");
            return Result.Success();
        }
        _logger.LogInformation("Integration in die Infuse-Mediathek abgeschlossen.");

        _logger.LogInformation("Final Cut Pro Workflow beendet.");
        _logger.LogInformation("--------------------------------------------------");
        return Result.Success();
    }
}