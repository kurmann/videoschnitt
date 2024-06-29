using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.HealthCheck.Services;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.HealthCheck;

public class HealthCheckFeature
{
    private readonly ToolsVersionService _toolsVersionService;
    private readonly ILogger<HealthCheckFeature> _logger;

    public HealthCheckFeature(ToolsVersionService toolsVersionService, ILogger<HealthCheckFeature> logger)
    {
        _toolsVersionService = toolsVersionService;
        _logger = logger;
    }

    public Result RunHealthCheck()
    {
        // Ermitteln der FFmpeg-Version
        _logger.LogInformation("Checking FFmpeg version...");
        var version = _toolsVersionService.GetFFmpegVersion();
        if (version.IsFailure)
        {
            return Result.Failure(version.Error);
        }
        
        _logger.LogInformation("FFmpeg version: {version}", version.Value);
        return Result.Success();
    }
}