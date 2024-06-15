using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

public class MediaIntegratorService
{
    private readonly ILogger<MediaIntegratorService> _logger;

    public MediaIntegratorService(ILogger<MediaIntegratorService> logger) => _logger = logger;

    public Result IntegrateMediaSet(IEnumerable<FileInfo> mediaSetFiles, DirectoryInfo targetDirectory, IEnumerable<string> suffixesToIntegrate)
    {
        if (mediaSetFiles == null || !mediaSetFiles.Any())
            return Result.Failure("MediaSetFiles darf nicht null oder leer sein.");
        if (targetDirectory == null)
            return Result.Failure("Das Zielverzeichnis muss angegeben sein.");
        if (suffixesToIntegrate == null || !suffixesToIntegrate.Any())
            return Result.Failure("SuffixesToIntegrate darf nicht null oder leer sein.");

        _logger.LogInformation($"Integriere Medienset in das Infuse-Mediathek-Verzeichnis {targetDirectory}.");

        // Erstelle das Zielverzeichnis, falls es nicht existiert
        try
        {
            if (!targetDirectory.Exists)
            {
                targetDirectory.Create();
            }
        }
        catch (Exception ex)
        {
            return Result.Failure($"Das Zielverzeichnis {targetDirectory} konnte nicht erstellt werden: {ex.Message}");
        }

        // Ermittle die Dateien im Medienset, die in das Infuse-Mediathek-Verzeichnis integriert werden sollen, ignoriere Groß-/Kleinschreibung und die Dateiendung
        var mediaSetFilesToMove = mediaSetFiles.Where(file => IsFileToMove(file, suffixesToIntegrate)).ToList();
        _logger.LogInformation($"Es wurden {mediaSetFilesToMove.Count} Dateien im Medienset gefunden, die in das Infuse-Mediathek-Verzeichnis integriert werden sollen.");

        // Bewege die betroffenen Dateien in das Infuse-Mediathek-Verzeichnis
        foreach (var mediaSetFile in mediaSetFilesToMove)
        {
            var targetFilePath = Path.Combine(targetDirectory.FullName, mediaSetFile.Name);
            try
            {
                mediaSetFile.MoveTo(targetFilePath);
                _logger.LogInformation($"Datei {mediaSetFile.FullName} wurde in das Infuse-Mediathek-Verzeichnis {targetDirectory} verschoben.");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Datei {mediaSetFile.FullName} konnte nicht in das Infuse-Mediathek-Verzeichnis {targetDirectory} verschoben werden: {ex.Message}");
            }
        }

        return Result.Success();
    }

    private bool IsFileToMove(FileInfo file, IEnumerable<string> suffixesToIntegrate)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
        return suffixesToIntegrate.Any(suffix => fileNameWithoutExtension.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
    }
}