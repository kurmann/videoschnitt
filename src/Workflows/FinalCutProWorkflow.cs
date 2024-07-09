using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Workflows.Abstractions;

namespace Kurmann.Videoschnitt.Workflows;

public class FinalCutProWorkflow : IAsyncWorkflow
{
    private readonly ILogger<FinalCutProWorkflow> _logger;
    private readonly MediaSetOrganizer.Engine _mediaSetOrganizerEngine;

    public FinalCutProWorkflow(ILogger<FinalCutProWorkflow> logger, MediaSetOrganizer.Engine mediaSetOrganizerEngine)
    {
        _logger = logger;
        _mediaSetOrganizerEngine = mediaSetOrganizerEngine;
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

        _logger.LogInformation("Final Cut Pro Workflow beendet.");
        _logger.LogInformation("--------------------------------------------------");
        return Result.Success();
    }
}