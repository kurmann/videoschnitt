using System.Diagnostics;
using System.Text;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.HealthCheck.Services;

public class ToolsVersionService
{
    private readonly ILogger<ToolsVersionService> _logger;
    private readonly ApplicationSettings _applicationSettings;

    public ToolsVersionService(ILogger<ToolsVersionService> logger, IOptions<ApplicationSettings> applicationSettings)
    {
        _logger = logger;
        _applicationSettings = applicationSettings.Value;
    }

    /// <summary>
    /// Führt den Befehl `ffmpeg -version` aus und gibt die Version zurück.
    /// </summary>
    /// <returns></returns>
    public Result<string> GetFFmpegVersion()
    {
        try
        {
            var ffmpegPath = _applicationSettings.ExternalTools.FFMpeg.Path;
            _logger.LogInformation("Verwende ffmpeg unter {ffmpegPath}", ffmpegPath);

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

    /// <summary>
    /// Führt den Befehl `sips --version`aus und gibt die Version zurück.
    /// SIPS ist ein Tool, das auf macOS verfügbar ist und für die Bildbearbeitung verwendet wird.
    /// </summary>
    /// <returns></returns>
    public Result<string> GetSipsVersion()
    {
        try
        {
            var sipsPath = _applicationSettings.ExternalTools.Sips.Path;
            _logger.LogInformation("Verwende sips unter {sipsPath}", sipsPath);

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{sipsPath} --version\"",
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