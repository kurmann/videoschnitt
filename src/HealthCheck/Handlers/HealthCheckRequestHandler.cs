using Kurmann.Videoschnitt.Messages.HealthCheck;

namespace Kurmann.Videoschnitt.HealthCheck.Handlers;

public class HealthCheckRequestHandler
{
    public Task HandleAsync(HealthCheckRequest _)
    {
        return Task.CompletedTask;
    }
}