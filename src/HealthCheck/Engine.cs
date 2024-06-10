using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kurmann.Videoschnitt.Messages.HealthCheck;
using Kurmann.Videoschnitt.HealthCheck.Services;
using Wolverine;

namespace Kurmann.Videoschnitt.HealthCheck;

public class Engine(ToolsVersionService toolsVersionService, IMessageBus messageBus)
{
    public async Task RunHealthCheckAsync()
    {
        // Ermitteln der FFmpeg-Version
        var version = toolsVersionService.GetFFmpegVersion();
        if (version.IsFailure)
        {
            await messageBus.PublishAsync(new HealthCheckFailedResponse("FFmpeg version could not be determined"));
            return;
        }
        await messageBus.PublishAsync(new HealthCheckResponse(version.Value));
    }
}