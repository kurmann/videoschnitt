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
        if (!integratedLocalInfuseMediaSet.HasIntegratedVideo)
        {
            return Result.Failure<IntegratedRemoteInfuseMediaSetDirectory>("Das Medienset enthält keine Videodatei. Es wird nichts verschoben.");
        }
        var sourceVideoFile = integratedLocalInfuseMediaSet.IntegratedVideoDetails.Value.SupportedVideo;
        var isVideoFileInUseResult = await _fileOperations.IsFileInUseAsync(sourceVideoFile);
        if (isVideoFileInUseResult.IsFailure)
        {
            return Result.Failure<IntegratedRemoteInfuseMediaSetDirectory>($"Fehler beim Prüfen, ob die Videodatei {sourceVideoFile} in Benutzung ist: {isVideoFileInUseResult.Error}");
        }
        if (isVideoFileInUseResult.Value)
        {
            _logger.LogInformation("Die Videodatei {VideoFile} ist in Benutzung. Die Datei wird nicht verschoben.", sourceVideoFile);
        }
        else
        {
            // Erstelle das Zielverzeichnis, falls es nicht existiert
            var targetDirectoryPath = Path.Combine(remoteInfuseMediaDirectory.FullName, integratedLocalInfuseMediaSet.IntegratedVideoDetails.Value.SubDirectory);
            if (!Directory.Exists(targetDirectoryPath))
            {
                Directory.CreateDirectory(targetDirectoryPath);
            }

            var targetVideoPath = Path.Combine(targetDirectoryPath, sourceVideoFile.Name);
            var moveResult = await _fileOperations.MoveFileAsync(sourceVideoFile, targetVideoPath, true, true);
            if (moveResult.IsFailure)
            {
                return Result.Failure<IntegratedRemoteInfuseMediaSetDirectory>($"Fehler beim Verschieben der Videodatei {sourceVideoFile} in das Infuse-Mediathek-Verzeichnis auf dem Medienserver (Netzwerkspeicher, bspw. NAS): {moveResult.Error}");
            }
            _logger.LogInformation("Videodatei {VideoFile} wurde erfolgreich in das Infuse-Mediathek-Verzeichnis auf dem Medienserver (Netzwerkspeicher, bspw. NAS) verschoben.", sourceVideoFile);
        }

        return new IntegratedRemoteInfuseMediaSetDirectory(remoteInfuseMediaDirectory);
    }
}

internal record IntegratedRemoteInfuseMediaSetDirectory(DirectoryInfo Directory);