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

    public Result<List<string>> RunHealthCheck()
    {
        // Ermitteln der FFmpeg-Version
        _logger.LogInformation("Checking FFmpeg version...");
        var version = _toolsVersionService.GetFFmpegVersion();
        if (version.IsFailure)
        {
            _logger.LogError("Error checking FFmpeg version: {Error}", version.Error);
            return Result.Failure<List<string>>(version.Error);
        }

        // Split result by line
        var lines = version.Value.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        return Result.Success(lines.ToList());
    }
}