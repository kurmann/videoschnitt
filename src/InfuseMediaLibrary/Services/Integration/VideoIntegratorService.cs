using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.Integration;

internal class VideoIntegratorService
{
    private readonly IFileOperations _fileOperations;
    private readonly ILogger<VideoIntegratorService> _logger;
    private readonly TargetPathService _targetPathService;

    public VideoIntegratorService(IFileOperations fileOperations, ILogger<VideoIntegratorService> logger, TargetPathService targetPathService)
    {
        _fileOperations = fileOperations;
        _logger = logger;
        _targetPathService = targetPathService;
    }

    public async Task<Result> IntegrateVideoAsync(FileInfo videoFile)
    {
        var targetDirectory = await _targetPathService.GetTargetDirectoryAsync(videoFile);
        if (targetDirectory.IsFailure)
        {
            return Result.Failure<Maybe<Result>>($"Das Zielverzeichnis für die Integration in die Infuse-Mediathek konnte nicht ermittelt werden: {targetDirectory.Value.FullName}");
        }
        if (!targetDirectory.Value.Exists)
        {
            _logger.LogInformation("Das Zielverzeichnis für die Integration in die Infuse-Mediathek existiert nicht. Erstelle Verzeichnis: {targetDirectory.FullName}", targetDirectory.Value.FullName);
            var createDirectoryResult = await _fileOperations.CreateDirectoryAsync(targetDirectory.Value.FullName);
            if (createDirectoryResult.IsFailure)
            {
                return Result.Failure<Maybe<Result>>($"Das Zielverzeichnis für die Integration in die Infuse-Mediathek konnte nicht erstellt werden: {targetDirectory.Value.FullName}. Fehler: {createDirectoryResult.Error}");
            }
        }

        var targetFileNameResult = _targetPathService.GetTargetFileName(videoFile);
        if (targetFileNameResult.IsFailure)
        {
            return Result.Failure<Maybe<Result>>($"Der Ziel-Dateiname für die Integration in die Infuse-Mediathek konnte nicht ermittelt werden: {targetFileNameResult.Error}");
        }

        // Erstelle den Ziel-Dateinamen als Komposition aus dem Zielverzeichnis und dem Dateinamen
        var targetFilePath = Path.Combine(targetDirectory.Value.FullName, targetFileNameResult.Value);

        // Erstelle das Unterzeichnis für die Integration in die Infuse-Mediathek
        if (!Directory.Exists(targetDirectory.Value.FullName))
        {
            _logger.LogInformation("Das Zielverzeichnis für die Integration in die Infuse-Mediathek existiert nicht. Erstelle Verzeichnis: {targetDirectory.FullName}", targetDirectory.Value.FullName);
            var createDirectoryResult = await _fileOperations.CreateDirectoryAsync(targetDirectory.Value.FullName);
            if (createDirectoryResult.IsFailure)
            {
                return Result.Failure<Maybe<Result>>($"Das Zielverzeichnis für die Integration in die Infuse-Mediathek konnte nicht erstellt werden: {targetDirectory.Value.FullName}. Fehler: {createDirectoryResult.Error}");
            }
        }

        // Verschiebe die Videodatei in das lokale Infuse-Mediathek-Verzeichnis und überschreibe die Datei falls sie bereits existiert
        var fileMoveResult = await _fileOperations.MoveFileAsync(videoFile.FullName, targetFilePath, true, true);
        if (fileMoveResult.IsFailure)
        {
            return Result.Failure<Maybe<Result>>($"Die Video-Datei {videoFile.FullName} konnte nicht in das Infuse-Mediathek-Verzeichnis {targetDirectory.Value.FullName} verschoben werden. Fehler: {fileMoveResult.Error}");
        }

        _logger.LogInformation("Die Video-Datei {videoFile} wurde in das Infuse-Mediathek-Verzeichnis {targetDirectory} verschoben.", videoFile.FullName, targetDirectory.Value.FullName);


        return Result.Success();
    }
}
