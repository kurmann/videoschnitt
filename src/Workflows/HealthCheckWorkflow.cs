using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.HealthCheck;

namespace Kurmann.Videoschnitt.Workflows;

public class HealthCheckWorkflow : IWorkflow
{
    private readonly HealthCheckFeature _healthCheckFeature;

    public HealthCheckWorkflow(HealthCheckFeature healthCheckFeature)
    {
        _healthCheckFeature = healthCheckFeature;
    }

    public Result Execute(IProgress<string> progress)
    {
        progress.Report("Health check started.");

        _healthCheckFeature.RunHealthCheck(progress);

        progress.Report("Health check completed.");
        return Result.Success();
    }
}