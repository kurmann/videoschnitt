using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder;

public class GenerateMediaSetIndexWorkflow
{
    public const string WorkflowName = "MediaSetIndex";

    private readonly ILogger<GenerateMediaSetIndexWorkflow> _logger;

    public GenerateMediaSetIndexWorkflow(ILogger<GenerateMediaSetIndexWorkflow> logger)
    {
        _logger = logger;
    }

    public Task<Result> ExecuteAsync()
    {
        _logger.LogInformation("Starting GenerateMediaSetIndexWorkflow...");

        _logger.LogInformation("GenerateMediaSetIndexWorkflow finished.");
        return Task.FromResult(Result.Success());
    }
}
