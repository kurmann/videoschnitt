using Kurmann.Videoschnitt.Messages.HealthCheck;
using Kurmann.Videoschnitt.HealthCheck.Services;

namespace Kurmann.Videoschnitt.HealthCheck.Handlers;

public class HealthCheckRequestHandler
{
    public Task HandleAsync(HealthCheckRequest _, Engine engine)
    {
        return engine.RunHealthCheckAsync();
    }
}