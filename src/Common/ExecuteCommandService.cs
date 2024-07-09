using System.Diagnostics;
using System.Text;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.Common;

public class ExecuteCommandService
{
    private readonly ILogger<ExecuteCommandService> _logger;

    public ExecuteCommandService(ILogger<ExecuteCommandService> logger) => _logger = logger;

    /// <summary>
    /// Führt den externen Process aus und retourniert das Ergebnis.
    /// Loggt die Rückgabewerte laufend.
    /// </summary>
    /// <param name="commandPath">Der Pfad zum auszuführenden Befehl.</param>
    /// <param name="arguments">Die Argumente für den Befehl.</param>
    /// <returns>Ein Result-Objekt, das den Erfolg oder Fehler enthält.</returns>
    public async Task<Result<List<string>>> ExecuteCommandAsync(string commandPath, string arguments)
    {
        _logger.LogTrace("Executing command: {commandPath} {arguments}", commandPath, arguments);

        var psi = new ProcessStartInfo(commandPath, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        var outputLines = new List<string>();
        var errorLines = new List<string>();

        using (var process = new Process { StartInfo = psi, EnableRaisingEvents = true })
        {
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    outputLines.Add(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errorLines.Add(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var errorMessage = string.Join(Environment.NewLine, errorLines);
                return Result.Failure<List<string>>($"Command '{commandPath} {arguments}' failed with exit code {process.ExitCode}: {errorMessage}");
            }
        }

        return Result.Success(outputLines);
    }

    /// <summary>
    /// Führt den externen Process aus und retourniert das Ergebnis bei dem ein Exit-Code ungleich 0 als False interpretiert wird.
    /// Dies ist bspw. bei lsof der Fall, wenn die Datei verwendet wird und ein Exit-Code von 0 zurückgegeben wird wenn kein Prozess die Datei verwendet.
    /// </summary>
    /// <param name="commandPath"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public async Task<bool> ExecuteBooleanCommandAsync(string commandPath, string arguments)
    {
        _logger.LogTrace("Executing command: {commandPath} {arguments}", commandPath, arguments);

        var commandResult = await ExecuteCommandAsync(commandPath, arguments);
        if (commandResult.IsFailure)
        {
            _logger.LogTrace("Prozess hat Exit-Code ungleich 0. Wird als True interpretiert.");
            return false;
        }

        return true;
    }
}