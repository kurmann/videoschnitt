using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.MetadataProcessor;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Workflows;

public class FinalCutProWorkflow : IAsyncWorkflow
{
    private readonly ILogger<FinalCutProWorkflow> _logger;
    private readonly MetadataProcessorEngine _metadataProcessorEngine;
    private readonly InfuseMediaLibrary.Engine _infuseMediaLibraryEngine;

    public FinalCutProWorkflow(ILogger<FinalCutProWorkflow> logger, MetadataProcessorEngine metadataProcessorEngine, InfuseMediaLibrary.Engine infuseMediaLibraryEngine)
    {
        _logger = logger;
        _metadataProcessorEngine = metadataProcessorEngine;
        _infuseMediaLibraryEngine = infuseMediaLibraryEngine;
    }

    public async Task<Result> ExecuteAsync(IProgress<string> progress)
    {
        progress.Report("Final Cut Pro Workflow gestartet.");

        var metadataProcessorResult = await _metadataProcessorEngine.Start(progress);
        if (metadataProcessorResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Ausführen des Final Cut Pro Workflows: {metadataProcessorResult.Error}");
        }

        var infuseMediaLibraryResult = _infuseMediaLibraryEngine.Start(progress);
        if (infuseMediaLibraryResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Ausführen des Final Cut Pro Workflows: {infuseMediaLibraryResult.Error}");
        }

        progress.Report("Final Cut Pro Workflow beendet.");
        return Result.Success();
    }
}
