using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using System.Text;
using Kurmann.Videoschnitt.CommonServices;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

public class MediaIntegratorService
{
    private readonly ILogger<MediaIntegratorService> _logger;
    private readonly FileTransferService _fileTransferService;

    public MediaIntegratorService(ILogger<MediaIntegratorService> logger, FileTransferService fileTransferService)
    {
        _logger = logger;
        _fileTransferService = fileTransferService;
    }

    public async Task<Result<IntegratedMediaSetFile>> IntegrateMediaSet(IEnumerable<FileInfo> mediaSetFiles,
                                                            DirectoryInfo targetDirectory,
                                                            IEnumerable<string> suffixesToIntegrate,
                                                            string recordingDateIsoString)
    {
        if (mediaSetFiles == null || !mediaSetFiles.Any())
            return Result.Failure<IntegratedMediaSetFile>("MediaSetFiles darf nicht null oder leer sein.");
        if (targetDirectory == null)
            return Result.Failure<IntegratedMediaSetFile>("TargetDirectory darf nicht null sein.");
        if (suffixesToIntegrate == null || !suffixesToIntegrate.Any())
            return Result.Failure<IntegratedMediaSetFile>("SuffixesToIntegrate darf nicht null oder leer sein.");
        if (string.IsNullOrWhiteSpace(recordingDateIsoString))
            return Result.Failure<IntegratedMediaSetFile>("RecordingDateIsoString darf nicht null oder leer sein.");

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
            return Result.Failure<IntegratedMediaSetFile>($"Das Infuse-Mediathek-Verzeichnis {targetDirectory} konnte nicht erstellt werden: {ex.Message}");
        }

        // Ermittle die Dateien im Medienset, die in das Infuse-Mediathek-Verzeichnis integriert werden sollen, ignoriere Groß-/Kleinschreibung und die Dateiendung
        var mediaSetFilesToMove = mediaSetFiles.Where(file => IsFileToMove(file, suffixesToIntegrate)).ToList();

        // Prüfe ob überhaupt eine Datei im Medienset gefunden wurde, die in das Infuse-Mediathek-Verzeichnis integriert werden soll
        if (!mediaSetFilesToMove.Any())
        {
            return Result.Failure<IntegratedMediaSetFile>("Es wurde keine Datei im Medienset gefunden, die in das Infuse-Mediathek-Verzeichnis integriert werden soll.");
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

            return Result.Failure<IntegratedMediaSetFile>(errorMessage);
        }

        var mediaSetFileToMove = mediaSetFilesToMove.First();
        
        // Die integrierte Datei im Infuse-Mediathek-Verzeichnis sollte den Dateinamen ohne Varianten-Suffix haben
        var filePathWithoutVariantSuffix = GetFileNameWithoutVariantSuffix(mediaSetFileToMove, suffixesToIntegrate);
        if (filePathWithoutVariantSuffix.IsFailure)
        {
            return Result.Failure<IntegratedMediaSetFile>($"Der Dateiname {mediaSetFileToMove.Name} konnte nicht ohne Varianten-Suffix ermittelt werden: {filePathWithoutVariantSuffix.Error}");
        }

        // Entferne das führende Aufnahmedatum aus dem Dateinamen (getrennt mit einem Leerzeichen vom Rest des Dateinamens) denn das Aufnahmedatum ist in den Infuse-Metadaten enthalten und das Verzeichnis wird danach benannt
        var fileNameWithoutRecordingDateAndVariantSuffix = filePathWithoutVariantSuffix.Value.Replace($"{recordingDateIsoString} ", string.Empty, StringComparison.OrdinalIgnoreCase);

        // Bewege die Datei in das Infuse-Mediathek-Verzeichnis
        try
        {   
            // Ermittle den Ziel-Pfad der Datei im Infuse-Mediathek-Verzeichnis
            var targetFilePath = Path.Combine(targetDirectory.FullName, fileNameWithoutRecordingDateAndVariantSuffix);

            mediaSetFileToMove.MoveTo(targetFilePath);
            _logger.LogInformation($"Die Datei {mediaSetFileToMove.FullName} wurde in das Infuse-Mediathek-Verzeichnis {targetDirectory} verschoben.");


            // Übertrage die Dateiberechtigungen von Quelle zu Ziel
            var transferPermissionsResult = await _fileTransferService.TransferPermissionsAsync(mediaSetFileToMove.FullName, targetFilePath);
            if (transferPermissionsResult.IsFailure)
            {
                return Result.Failure<IntegratedMediaSetFile>($"Die Dateiberechtigungen von {mediaSetFileToMove.FullName} konnten nicht auf {targetFilePath} übertragen werden: {transferPermissionsResult.Error}");
            }

            return Result.Success(new IntegratedMediaSetFile(mediaSetFileToMove, new FileInfo(targetFilePath)));
        }
        catch (Exception ex)
        {
            return Result.Failure<IntegratedMediaSetFile>($"Die Datei {mediaSetFileToMove.FullName} konnte nicht in das Infuse-Mediathek-Verzeichnis {targetDirectory} verschoben werden: {ex.Message}");
        }
    }

    private bool IsFileToMove(FileInfo file, IEnumerable<string> suffixesToIntegrate)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
        return suffixesToIntegrate.Any(suffix => fileNameWithoutExtension.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
    }

    private Result<string> GetFileNameWithoutVariantSuffix(FileInfo file, IEnumerable<string> suffixesToIntegrate)
    {
        // Ermittle das Verzeichnis der Datei
        var fileDirectory = file.Directory;
        if (fileDirectory == null)
        {
            return Result.Failure<string>($"Das Verzeichnis der Datei {file.FullName} konnte nicht ermittelt werden.");
        }

        foreach (var suffix in suffixesToIntegrate)
        {
            if (Path.GetFileNameWithoutExtension(file.FullName).EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                // Der Dateiname enthält das Varianten-Suffix, entferne es
                var fileNameWithoutVariantSuffix = Path.Combine(Path.GetFileNameWithoutExtension(file.Name).Replace(suffix, string.Empty, StringComparison.OrdinalIgnoreCase) + file.Extension);
                return Result.Success(fileNameWithoutVariantSuffix);
            }
        }

        return Result.Failure<string>($"Der Dateiname {file.Name} enthält kein Varianten-Suffix.");
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