using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.MetadataProcessor;

namespace Kurmann.Videoschnitt.Application.Workflows;

public class FinalCutProWorkflow(ILogger<FinalCutProWorkflow> logger, MetadataProcessorEngine engine) : IAsyncWorkflow
{
    public async Task<Result> ExecuteAsync()
    {
        logger.LogInformation("Final Cut Pro Workflow gestartet.");

        await engine.StartAsync();

        logger.LogInformation("Final Cut Pro Workflow beendet.");
        return Result.Success();
    }
}