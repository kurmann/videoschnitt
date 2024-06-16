using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.CommonServices;

namespace Kurmann.Videoschnitt.LocalFileSystem.UnixSystems;

/// <summary>
/// Service zum Übertragen von Dateiberechtigungen auf Unix-Systemen.
/// </summary>
public class FilePermissionsService
{
    private readonly ExecuteCommandService _executeCommandService;

    public FilePermissionsService(ExecuteCommandService executeCommandService)
    {
        _executeCommandService = executeCommandService;
    }

    /// <summary>
    /// Überträgt die Berechtigungen von einer Datei auf eine andere.
    /// </summary>
    /// <param name="sourcePath">Der Pfad der Quell-Datei.</param>
    /// <param name="destinationPath">Der Pfad der Ziel-Datei.</param>
    /// <returns>Ein Result-Objekt, das den Erfolg oder Fehler enthält.</returns>
    public async Task<Result> TransferPermissionsAsync(string sourcePath, string destinationPath)
    {
        var chmodResult = await _executeCommandService.ExecuteCommandAsync("chmod", $"--reference=\"{sourcePath}\" \"{destinationPath}\"");
        if (chmodResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Übertragen der Berechtigungen: {chmodResult.Error}");
        }

        var chownResult = await _executeCommandService.ExecuteCommandAsync("chown", $"--reference=\"{sourcePath}\" \"{destinationPath}\"");
        if (chownResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Übertragen des Eigentümers: {chownResult.Error}");
        }

        return Result.Success();
    }

    /// <summary>
    /// Überträgt die Berechtigungen von einer Referenzdatei auf alle Dateien in einem Verzeichnis.
    /// </summary>
    /// <param name="referenceFilePath">Der Pfad der Referenzdatei.</param>
    /// <param name="targetDirectory">Das Zielverzeichnis.</param>
    /// <returns>Ein Result-Objekt, das den Erfolg oder Fehler enthält.</returns>
    public async Task<Result> TransferDirectoryPermissionsAsync(string referenceFilePath, string targetDirectory)
    {
        if (!File.Exists(referenceFilePath))
        {
            return Result.Failure($"Referenzdatei '{referenceFilePath}' existiert nicht.");
        }

        if (!Directory.Exists(targetDirectory))
        {
            return Result.Failure($"Zielverzeichnis '{targetDirectory}' existiert nicht.");
        }

        var targetDirInfo = new DirectoryInfo(targetDirectory);

        foreach (var targetFile in targetDirInfo.GetFiles("*", SearchOption.AllDirectories))
        {
            var fileResult = await TransferPermissionsAsync(referenceFilePath, targetFile.FullName);
            if (fileResult.IsFailure)
            {
                return fileResult;
            }
        }

        foreach (var targetSubDir in targetDirInfo.GetDirectories("*", SearchOption.AllDirectories))
        {
            var subDirResult = await TransferPermissionsAsync(referenceFilePath, targetSubDir.FullName);
            if (subDirResult.IsFailure)
            {
                return subDirResult;
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// Löscht spezifische Berechtigungen in einem Verzeichnis, sodass die Berechtigungen von der Verzeichnisstruktur oberhalb übernommen werden.
    /// </summary>
    /// <param name="directory">Das Zielverzeichnis.</param>
    /// <returns>Ein Result-Objekt, das den Erfolg oder Fehler enthält.</returns>
    public async Task<Result> ClearSpecificPermissionsAsync(DirectoryInfo directory)
    {
        if (!directory.Exists)
        {
            return Result.Failure($"Verzeichnis '{directory.FullName}' existiert nicht.");
        }

        foreach (var targetFile in directory.GetFiles("*", SearchOption.AllDirectories))
        {
            var chmodResult = await _executeCommandService.ExecuteCommandAsync("chmod", $"u+rX,g+rX,o+rX \"{targetFile.FullName}\"");
            if (chmodResult.IsFailure)
            {
                return chmodResult;
            }
        }

        foreach (var targetSubDir in directory.GetDirectories("*", SearchOption.AllDirectories))
        {
            var chmodResult = await _executeCommandService.ExecuteCommandAsync("chmod", $"u+rX,g+rX,o+rX \"{targetSubDir.FullName}\"");
            if (chmodResult.IsFailure)
            {
                return chmodResult;
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// Löscht spezifische Berechtigungen für eine Datei, sodass die Berechtigungen von der Verzeichnisstruktur oberhalb übernommen werden.
    /// </summary>
    /// <param name="file">Die Zieldatei.</param>
    /// <returns>Ein Result-Objekt, das den Erfolg oder Fehler enthält.</returns>
    public async Task<Result> ClearSpecificPermissionsAsync(FileInfo file)
    {
        if (!file.Exists)
        {
            return Result.Failure($"Datei '{file.FullName}' existiert nicht.");
        }

        var chmodResult = await _executeCommandService.ExecuteCommandAsync("chmod", $"u+rX,g+rX,o+rX \"{file.FullName}\"");
        return chmodResult;
    }
}