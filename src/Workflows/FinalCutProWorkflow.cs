using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Workflows;

public class FinalCutProWorkflow : IAsyncWorkflow
{
    private readonly ILogger<FinalCutProWorkflow> _logger;
    private readonly MetadataProcessor.Engine _metadataProcessorEngine;
    private readonly InfuseMediaLibrary.Engine _infuseMediaLibraryEngine;

    public FinalCutProWorkflow(ILogger<FinalCutProWorkflow> logger, MetadataProcessor.Engine metadataProcessorEngine, InfuseMediaLibrary.Engine infuseMediaLibraryEngine)
    {
        _logger = logger;
        _metadataProcessorEngine = metadataProcessorEngine;
        _infuseMediaLibraryEngine = infuseMediaLibraryEngine;
    }

    public async Task<Result> ExecuteAsync()
    {
        _logger.LogInformation("Final Cut Pro Workflow gestartet.");


        _logger.LogInformation("Starte Metadaten-Verarbeitung");
        var metadataProcessorResult = await _metadataProcessorEngine.StartAsync();
        if (metadataProcessorResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Ausführen des Final Cut Pro Workflows: {metadataProcessorResult.Error}");
        }

        _logger.LogInformation("Starte Integration in die Infuse-Mediathek");

        var integratedMediaServerFilesByMediaSet = await _infuseMediaLibraryEngine.StartAsync(metadataProcessorResult.Value);
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
        return Result.Success();
    }
}