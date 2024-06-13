using System.Diagnostics;
using System.Text;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

public class CommandExecutorService
{
    private readonly ILogger<CommandExecutorService> _logger;

    public CommandExecutorService(ILogger<CommandExecutorService> logger) => _logger = logger;

    /// <summary>
    /// Führt den externen Process aus und retourniert alle Rückgabewerte.
    /// Loggt die Rückgabewerte laufend.
    /// </summary>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public async Task<Result<List<string>>> ExecuteCommandAsync(string commandPath, string arguments)
    {
        _logger.LogInformation($"Executing command: {commandPath} {arguments}");

        var psi = new ProcessStartInfo(commandPath, arguments)
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
        };

        var lines = new List<string>();

        using (var process = new Process { StartInfo = psi, EnableRaisingEvents = true })
        {
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    _logger.LogInformation(e.Data);
                    lines.Add(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();
        }

        return Result.Success(lines);
    }
}