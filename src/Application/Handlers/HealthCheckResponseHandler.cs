using Microsoft.AspNetCore.SignalR;
using Kurmann.Videoschnitt.Messages.HealthCheck;

namespace Kurmann.Videoschnitt.Application.Handlers;

public class HealthCheckResponseHandler(IHubContext<LogHub> logHubContext)
{
    public async Task Handle(HealthCheckResponse message)
    {
        var logMessage = $"FFmpeg-Version: {message.FFmpegVersionInfo}";
        await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);
    }
}