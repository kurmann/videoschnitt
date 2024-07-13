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

        // todo: Dateiname generieren und video verschieben
        var targetFileNameResult = _targetPathService.GetTargetFileName(videoFile);

        return targetFileNameResult;
    }
}
