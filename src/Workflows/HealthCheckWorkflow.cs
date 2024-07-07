using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.HealthCheck;
using Kurmann.Videoschnitt.Workflows.Abstractions;

namespace Kurmann.Videoschnitt.Workflows;

public class HealthCheckWorkflow : ISyncWorkflow
{
    public const string WorkflowName = "HealthCheck";

    private readonly Engine _healthCheckFeature;
    private readonly ILogger<HealthCheckWorkflow> _logger;

    public HealthCheckWorkflow(Engine healthCheckFeature, ILogger<HealthCheckWorkflow> logger)
    {
        _healthCheckFeature = healthCheckFeature;
        _logger = logger;
    }


    public Result Execute()
    {
        _logger.LogInformation("Health check started.");

        var healthCheckResult = _healthCheckFeature.RunHealthCheck();
        if (healthCheckResult.IsFailure)
        {
            return Result.Failure($"Error running health check: {healthCheckResult.Error}");
        }

        // log result
        _logger.LogInformation("FFmpeg version: {FFmpegVersion}", healthCheckResult.Value.FFmpegVersion);
        _logger.LogInformation("Sips version: {SipsVersion}", healthCheckResult.Value.SipsVersion);

        _logger.LogInformation("Health check finished.");
        return Result.Success();
    }
}