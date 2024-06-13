using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.MetadataProcessor;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Workflows;

public class FinalCutProWorkflow : IWorkflow
{
    private readonly ILogger<FinalCutProWorkflow> _logger;
    private readonly MetadataProcessorEngine _metadataProcessorEngine;

    public FinalCutProWorkflow(ILogger<FinalCutProWorkflow> logger, MetadataProcessorEngine metadataProcessorEngine)
    {
        _logger = logger;
        _metadataProcessorEngine = metadataProcessorEngine;
    }

    public Result Execute(IProgress<string> progress)
    {
        progress.Report("Final Cut Pro Workflow gestartet.");

        var result = _metadataProcessorEngine.Start(progress);
        if (result.IsFailure)
        {
            return Result.Failure($"Fehler beim Ausführen des Final Cut Pro Workflows: {result.Error}");
        }

        progress.Report("Final Cut Pro Workflow beendet.");
        return Result.Success();
    }
}
