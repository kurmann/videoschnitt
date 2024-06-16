using System.Diagnostics;
using System.Text;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

/// <summary>
/// Verwaltet das Kopieren und Verschieben von Dateien.
/// Gegenüber dem .NET Framework bietet .NET Core keine Möglichkeit, die Berechtigungen einer Datei zu übernehmen,
/// da es sich bei .NET Core um eine plattformübergreifende Bibliothek handelt und die Dateiberechtigungen
/// auf Windows und Linux unterschiedlich sind.
/// Dieser Service umgeht das Problem, indem er den Befehl `cp -p` oder `mv -p` ausführt.
/// </summary>
public class FileTransferService
{
    private readonly ILogger<FileTransferService> _logger;

    public FileTransferService(ILogger<FileTransferService> logger) => _logger = logger;

    /// <summary>
    /// Kopiert eine Datei und übernimmt die Berechtigungen.
    /// </summary>
    /// <param name="sourcePath">Der Quellpfad der Datei.</param>
    /// <param name="destinationPath">Der Zielpfad der Datei.</param>
    /// <returns>Ein Result-Objekt, das den Erfolg oder Fehler enthält.</returns>
    public async Task<Result> CopyFileWithPermissionsAsync(string sourcePath, string destinationPath)
    {
        return await ExecuteCommandAsync("cp", $"-p \"{sourcePath}\" \"{destinationPath}\"");
    }

    /// <summary>
    /// Verschiebt eine Datei und übernimmt die Berechtigungen.
    /// </summary>
    /// <param name="sourcePath">Der Quellpfad der Datei.</param>
    /// <param name="destinationPath">Der Zielpfad der Datei.</param>
    /// <returns>Ein Result-Objekt, das den Erfolg oder Fehler enthält.</returns>
    public async Task<Result> MoveFileWithPermissionsAsync(string sourcePath, string destinationPath)
    {
        return await ExecuteCommandAsync("mv", $"-p \"{sourcePath}\" \"{destinationPath}\"");
    }

    /// <summary>
    /// Führt den externen Process aus und retourniert das Ergebnis.
    /// Loggt die Rückgabewerte laufend.
    /// </summary>
    /// <param name="command">Der auszuführende Befehl.</param>
    /// <param name="arguments">Die Argumente des Befehls.</param>
    /// <returns>Ein Result-Objekt, das den Erfolg oder Fehler enthält.</returns>
    private async Task<Result> ExecuteCommandAsync(string command, string arguments)
    {
        _logger.LogInformation($"Executing command: {command} {arguments}");

        var psi = new ProcessStartInfo(command, arguments)
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
                    _logger.LogInformation(e.Data);
                    outputLines.Add(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    _logger.LogError(e.Data);
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
                return Result.Failure($"Command '{command} {arguments}' failed with exit code {process.ExitCode}: {errorMessage}");
            }
        }

        return Result.Success();
    }
}