using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.Workflows;

public class FinalCutProWorkflow
{
    private readonly ILogger<FinalCutProWorkflow> _logger;

    public FinalCutProWorkflow(ILogger<FinalCutProWorkflow> logger) => _logger = logger;

    public async Task ExecuteAsync(IProgress<string> progress)
    {
        progress.Report("Metadata processing started.");

        // Simuliere die Metadatenverarbeitung
        await Task.Delay(1000);
        progress.Report("Step 1 completed.");

        await Task.Delay(1000);
        progress.Report("Step 2 completed.");

        progress.Report("Metadata processing completed.");
    }
}
