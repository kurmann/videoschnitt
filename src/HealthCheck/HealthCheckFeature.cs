using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kurmann.Videoschnitt.Messages.HealthCheck;
using Kurmann.Videoschnitt.HealthCheck.Services;
using Wolverine;

namespace Kurmann.Videoschnitt.HealthCheck;

public class HealthCheckFeature
{
    private readonly ToolsVersionService _toolsVersionService;
    private readonly IMessageBus _messageBus;

    public HealthCheckFeature(ToolsVersionService toolsVersionService, IMessageBus messageBus)
    {
        _toolsVersionService = toolsVersionService;
        _messageBus = messageBus;
    }

    public async Task RunHealthCheckAsync()
    {
        // Ermitteln der FFmpeg-Version
        var version = _toolsVersionService.GetFFmpegVersion();
        if (version.IsFailure)
        {
            await _messageBus.PublishAsync(new HealthCheckFailedResponse("FFmpeg version could not be determined"));
            return;
        }
        await _messageBus.PublishAsync(new HealthCheckResponse(version.Value));
    }
}