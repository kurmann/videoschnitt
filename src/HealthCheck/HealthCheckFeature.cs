using Kurmann.Videoschnitt.HealthCheck.Services;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.HealthCheck;

public class HealthCheckFeature
{
    private readonly ToolsVersionService _toolsVersionService;

    public HealthCheckFeature(ToolsVersionService toolsVersionService)
    {
        _toolsVersionService = toolsVersionService;
    }

    public Result RunHealthCheck(IProgress<string> progress)
    {
        // Ermitteln der FFmpeg-Version
        progress.Report("Checking FFmpeg version...");
        var version = _toolsVersionService.GetFFmpegVersion();
        if (version.IsFailure)
        {
            progress.Report("Failed to check FFmpeg version.");
            return Result.Failure(version.Error);
        }
        
        progress.Report(version.Value);
        return Result.Success();
    }
}