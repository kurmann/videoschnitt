using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Workflows
{
    public class HealthCheckWorkflow : IAsyncWorkflow
    {
        public async Task<Result> ExecuteAsync(IProgress<string> progress)
        {
            progress.Report("Health check started.");

            // Simulate health check
            await Task.Delay(1000);
            progress.Report("Step 1 completed.");

            await Task.Delay(1000);
            progress.Report("Step 2 completed.");

            progress.Report("Health check completed.");

            return Result.Success();
        }
    }
}