using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Application.Workflows.Models;
using Kurmann.Videoschnitt.MetadataProcessor;

namespace Kurmann.Videoschnitt.Application.Workflows;

public class FinalCutProWorkflow(ILogger<FinalCutProWorkflow> logger, MetadataProcessorEngine metadataProcessorEngine) : IAsyncWorkflow
{
    public async Task<Result> ExecuteAsync(Action<StatusUpdate> statusCallback)
    {
        logger.LogInformation("Final Cut Pro Workflow gestartet.");

        await metadataProcessorEngine.StartAsync();

        // Erste Statusaktualisierung
        statusCallback(new StatusUpdate("Processing started"));

        // Simuliere einen Verarbeitungsschritt
        await Task.Delay(1000); // Simuliert eine asynchrone Arbeit
        statusCallback(new StatusUpdate("Step 1 completed"));

        // Simuliere einen weiteren Verarbeitungsschritt
        await Task.Delay(1000); // Simuliert eine asynchrone Arbeit
        statusCallback(new StatusUpdate("Step 2 completed"));

        // Abschließende Statusaktualisierung
        statusCallback(new StatusUpdate("Processing completed"));

        logger.LogInformation("Final Cut Pro Workflow beendet.");
        return Result.Success();
    }
}