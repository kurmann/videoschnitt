using Microsoft.AspNetCore.SignalR;
using Kurmann.Videoschnitt.Messages.HealthCheck;

namespace Kurmann.Videoschnitt.Application.Handlers;

public class HealthCheckRequestHandler(IHubContext<LogHub> logHubContext)
{
    public async Task Handle(HealthCheckRequest _)
    {
        var logMessage = "Anfrage für Statusabfrage (HealthCheck) erhalten.";
        await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);
    }
}