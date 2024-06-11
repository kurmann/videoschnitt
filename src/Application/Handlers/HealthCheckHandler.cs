using Microsoft.AspNetCore.SignalR;
using Kurmann.Videoschnitt.Messages.HealthCheck;

namespace Kurmann.Videoschnitt.Application.Handlers;

public class HealthCheckHandler(IHubContext<LogHub> logHubContext)
{
    public async Task Handle(HealthCheckRequest _)
    {
        var logMessage = "Anfrage für Statusabfrage (HealthCheck) erhalten.";
        await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);
    }

    public async Task Handle(HealthCheckResponse message)
    {
        var logMessage = $"FFmpeg-Version: {message.FFmpegVersionInfo}";
        await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);
    }
}