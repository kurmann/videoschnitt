using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.HealthCheck.Services;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.HealthCheck;

public class Engine
{
    private readonly ToolsVersionService _toolsVersionService;
    private readonly ILogger<Engine> _logger;

    public Engine(ToolsVersionService toolsVersionService, ILogger<Engine> logger)
    {
        _toolsVersionService = toolsVersionService;
        _logger = logger;
    }

    public Result<string> RunHealthCheck()
    {
        // Ermitteln der FFmpeg-Version
        _logger.LogInformation("Checking FFmpeg version...");
        var version = _toolsVersionService.GetFFmpegVersion();
        if (version.IsFailure)
        {
            return Result.Failure<string>(version.Error);
        }
        
        _logger.LogInformation("FFmpeg version: {version}", version.Value);
        return Result.Success(version.Value);
    }
}