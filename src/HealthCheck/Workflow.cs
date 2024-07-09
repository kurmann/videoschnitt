using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.HealthCheck.Services;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.HealthCheck;

public class Workflow
{
    public const string WorkflowName = "HealthCheck";

    private readonly ToolsVersionService _toolsVersionService;
    private readonly ILogger<Workflow> _logger;

    public Workflow(ToolsVersionService toolsVersionService, ILogger<Workflow> logger)
    {
        _toolsVersionService = toolsVersionService;
        _logger = logger;
    }

    public Result<HealthCheckResonse> ExecuteAsync()
    {
        // Ermitteln der FFmpeg-Version
        _logger.LogInformation("Checking FFmpeg version...");
        var version = _toolsVersionService.GetFFmpegVersion();
        if (version.IsFailure)
        {
            return Result.Failure<HealthCheckResonse>($"Error checking FFmpeg version: {version.Error}");
        }
        if (string.IsNullOrWhiteSpace(version.Value))
        {
            return Result.Failure<HealthCheckResonse>($"Checking FFMpeg version returned empty result.");
        }
        var ffmpegVersion = version.Value;

        // Ermitteln der SIPS-Version
        _logger.LogInformation("Checking SIPS version...");
        version = _toolsVersionService.GetSipsVersion();
        if (version.IsFailure)
        {
            return Result.Failure<HealthCheckResonse>($"Error checking SIPS version: {version.Error}");
        }
        if (string.IsNullOrWhiteSpace(version.Value))
        {
            return Result.Failure<HealthCheckResonse>($"Checking SIPS version returned empty result.");
        }
        var sipsVersion = version.Value;

        // Rückgabe der Versionen
        return Result.Success(new HealthCheckResonse(ffmpegVersion, sipsVersion));
    }
}

public record HealthCheckResonse(string FFmpegVersion, string SipsVersion);