using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.LocalIntegration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.RemoteIntegration;

/// <summary>
/// Verwantwortlich für die Integration von Infuse-Medien aus der lokalen Infuse-Mediathek in die Infuse-Mediathek auf dem Medienserver (Netzwerkspeicher, bspw. NAS)
/// </summary>
internal class InfuseMediaIntegrator
{
    private readonly ILogger<InfuseMediaIntegrator> _logger;
    private readonly InfuseMediaLibrarySettings _infuseMediaLibrarySettings;
    private readonly IFileOperations _fileOperations;

    public InfuseMediaIntegrator(ILogger<InfuseMediaIntegrator> logger, IOptions<InfuseMediaLibrarySettings> infuseMediaLibrarySettings, IFileOperations fileOperations)
    {
        _logger = logger;
        _infuseMediaLibrarySettings = infuseMediaLibrarySettings.Value;
        _fileOperations = fileOperations;
    }


    public async Task<Result<IntegratedRemoteInfuseMediaSetDirectory>> IntegrateInfuseMediaAsync(IntegratedLocalInfuseMediaSet integratedLocalInfuseMediaSet)
    {
        // Breche ab wenn das remote Infuse-Mediathek-Verzeichnis nicht konfiguriert ist oder nicht existiert
        if (string.IsNullOrWhiteSpace(_infuseMediaLibrarySettings.InfuseMediaLibraryPathRemote))
        {
            return Result.Failure<IntegratedRemoteInfuseMediaSetDirectory>("Das Infuse-Mediathek-Verzeichnis auf dem Medienserver (Netzwerkspeicher, bspw. NAS) ist nicht konfiguriert.");
        }
        var remoteInfuseMediaDirectory = new DirectoryInfo(_infuseMediaLibrarySettings.InfuseMediaLibraryPathRemote);
        if (!remoteInfuseMediaDirectory.Exists)
        {
            return Result.Failure<IntegratedRemoteInfuseMediaSetDirectory>($"Das Infuse-Mediathek-Verzeichnis auf dem Medienserver (Netzwerkspeicher, bspw. NAS) existiert nicht: {remoteInfuseMediaDirectory.FullName}");
        }

        // Verschiebe die Videodatei aus dem lokalen Infuse-Mediathek-Verzeichnis in das Infuse-Mediathek-Verzeichnis auf dem Medienserver (Netzwerkspeicher, bspw. NAS)
        var sourceVideoFile = integratedLocalInfuseMediaSet.Video;
        if (sourceVideoFile.HasNoValue)
        {
            return Result.Failure<IntegratedRemoteInfuseMediaSetDirectory>("Die Videodatei ist leer.");
        }
        var isVideoFileInUseResult = await _fileOperations.IsFileInUseAsync(sourceVideoFile.Value);
        if (isVideoFileInUseResult.IsFailure)
        {
            return Result.Failure<IntegratedRemoteInfuseMediaSetDirectory>($"Fehler beim Prüfen, ob die Videodatei {sourceVideoFile.Value} in Benutzung ist: {isVideoFileInUseResult.Error}");
        }
        if (isVideoFileInUseResult.Value)
        {
            _logger.LogInformation("Die Videodatei {VideoFile} ist in Benutzung. Die Datei wird nicht verschoben.", sourceVideoFile.Value);
        }
        else
        {
            var targetVideoPath = Path.Combine(remoteInfuseMediaDirectory.FullName, sourceVideoFile.Value);
            var moveResult = await _fileOperations.MoveFileAsync(sourceVideoFile.Value, targetVideoPath, true, true);
            if (moveResult.IsFailure)
            {
                return Result.Failure<IntegratedRemoteInfuseMediaSetDirectory>($"Fehler beim Verschieben der Videodatei {sourceVideoFile.Value} in das Infuse-Mediathek-Verzeichnis auf dem Medienserver (Netzwerkspeicher, bspw. NAS): {moveResult.Error}");
            }
            _logger.LogInformation("Videodatei {VideoFile} wurde erfolgreich in das Infuse-Mediathek-Verzeichnis auf dem Medienserver (Netzwerkspeicher, bspw. NAS) verschoben.", sourceVideoFile.Value);
        }

        return new IntegratedRemoteInfuseMediaSetDirectory(remoteInfuseMediaDirectory);
    }
}

internal record IntegratedRemoteInfuseMediaSetDirectory(DirectoryInfo Directory);