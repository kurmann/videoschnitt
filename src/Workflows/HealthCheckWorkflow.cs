using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.HealthCheck;
using Kurmann.Videoschnitt.Workflows.Abstractions;

namespace Kurmann.Videoschnitt.Workflows;

public class HealthCheckWorkflow : ISyncWorkflow
{
    public const string WorkflowName = "HealthCheck";

    private readonly HealthCheckFeature _healthCheckFeature;
    private readonly ILogger<HealthCheckWorkflow> _logger;

    public HealthCheckWorkflow(HealthCheckFeature healthCheckFeature, ILogger<HealthCheckWorkflow> logger)
    {
        _healthCheckFeature = healthCheckFeature;
        _logger = logger;
    }


    public Result Execute()
    {
        _logger.LogInformation("Health check started.");

        _healthCheckFeature.RunHealthCheck();

        _logger.LogInformation("Health check finished.");
        return Result.Success();
    }
}