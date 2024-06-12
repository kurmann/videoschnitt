using Microsoft.AspNetCore.SignalR;
using Kurmann.Videoschnitt.Messages.HealthCheck;

namespace Kurmann.Videoschnitt.Application.Handlers;

public class HealthCheckHandler
{
    private readonly IHubContext<LogHub> logHubContext;

    public HealthCheckHandler(IHubContext<LogHub> logHubContext)
    {
        this.logHubContext = logHubContext;
    }

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