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
        if (healthCheckResult.Value.Count == 0)
        {
            _logger.LogError("Health check result is empty.");
            return Result.Failure("Health check failed.");
        }

        // log all lines
        foreach (var line in healthCheckResult.Value)
        {
            _logger.LogInformation(line);
        }

        _logger.LogInformation("Health check finished.");
        return Result.Success();
    }
}