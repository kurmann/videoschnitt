using System.Diagnostics;
using System.Text;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.HealthCheck.Services;

public class ToolsVersionService
{
    private readonly ILogger<ToolsVersionService> _logger;

    public ToolsVersionService(ILogger<ToolsVersionService> logger)
    {
        _logger = logger;
    }

    public Result<string> GetFFmpegVersion()
    {
        try
        {
            var ffmpegPath = "/usr/local/bin/ffmpeg"; // Vollständiger Pfad zu ffmpeg

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{ffmpegPath} -version\"",
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            _logger.LogInformation("Executing command: {Command}", process.StartInfo.Arguments);

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(ex.Message);
        }
    }
}