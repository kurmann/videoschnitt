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

    public async Task<Result> ExecuteAsync(IProgress<string> progress)
    {
        progress.Report("Final Cut Pro Workflow gestartet.");

        progress.Report(Environment.NewLine);
        progress.Report("Starte Metadaten-Verarbeitung");
        var metadataProcessorResult = await _metadataProcessorEngine.Start(progress);
        if (metadataProcessorResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Ausführen des Final Cut Pro Workflows: {metadataProcessorResult.Error}");
        }

        progress.Report(Environment.NewLine);
        progress.Report("Starte Integration in die Infuse-Mediathek");

        var infuseMediaLibraryResult = await _infuseMediaLibraryEngine.StartAsync(progress, metadataProcessorResult.Value);
        if (infuseMediaLibraryResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Ausführen des Final Cut Pro Workflows: {infuseMediaLibraryResult.Error}");
        }

        progress.Report("Final Cut Pro Workflow beendet.");
        return Result.Success();
    }
}