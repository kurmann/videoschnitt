using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using System.Text;

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

        // Prüfe ob überhaupt eine Datei im Medienset gefunden wurde, die in das Infuse-Mediathek-Verzeichnis integriert werden soll
        if (!mediaSetFilesToMove.Any())
        {
            return Result.Failure("Es wurde keine Datei im Medienset gefunden, die in das Infuse-Mediathek-Verzeichnis integriert werden soll.");
        }

        // Für die Integration in die Infuse-Mediathek sollte nur eine Datei unter den Dateien im Medienset vorhanden sein
        if (mediaSetFilesToMove.Count > 1)
        {
            var stringBuilder = new StringBuilder();
            var errorMessage = stringBuilder.Append("Es wurde mehr als eine Datei im Medienset gefunden, die in das Infuse-Mediathek-Verzeichnis integriert werden soll.")
                .Append("Dies ist nicht vorgesehen, da die Infuse-Mediathek mehrere Versionen von Eigenproduktionen nicht zufriedenstellend unterstützt.")
                .Append("Die Dateien sind: ")
                .Append(string.Join(", ", mediaSetFilesToMove))
                .ToString();

            return Result.Failure(errorMessage);
        }

        var mediaSetFileToMove = mediaSetFilesToMove.First();
        
        // Die integrierte Datei im Infuse-Mediathek-Verzeichnis sollte den Dateinamen ohne Varianten-Suffix haben
        var fileNameWithoutVariantSuffix = GetFileNameWithoutVariantSuffix(mediaSetFileToMove.Name, suffixesToIntegrate);

        // Bewege die Datei in das Infuse-Mediathek-Verzeichnis
        var targetFilePath = Path.Combine(targetDirectory.FullName, fileNameWithoutVariantSuffix);
        try
        {   
            mediaSetFileToMove.MoveTo(targetFilePath);
            _logger.LogInformation($"Datei {mediaSetFileToMove.FullName} wurde in das Infuse-Mediathek-Verzeichnis {targetDirectory} verschoben.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Datei {mediaSetFileToMove.FullName} konnte nicht in das Infuse-Mediathek-Verzeichnis {targetDirectory} verschoben werden: {ex.Message}");
        }

        return Result.Success();
    }

    private bool IsFileToMove(FileInfo file, IEnumerable<string> suffixesToIntegrate)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
        return suffixesToIntegrate.Any(suffix => fileNameWithoutExtension.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
    }

    private string GetFileNameWithoutVariantSuffix(string fileName, IEnumerable<string> suffixesToIntegrate)
    {
        foreach (var suffix in suffixesToIntegrate)
        {
            if (fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return fileName.Substring(0, fileName.Length - suffix.Length);
            }
        }

        return fileName;
    }
}

public record IntegratedMediaSetFile
{
    /// <summary>
    /// Die Quelldatei, die in das Infuse-Mediathek-Verzeichnis integriert wurde.
    /// </summary>
    public FileInfo SourceFile { get; }

    /// <summary>
    /// Die Zieldatei im Infuse-Mediathek-Verzeichnis.
    /// </summary>
    public FileInfo TargetFile { get; }

    public IntegratedMediaSetFile(FileInfo sourceFile, FileInfo targetFile)
    {
        SourceFile = sourceFile;
        TargetFile = targetFile;
    }
}