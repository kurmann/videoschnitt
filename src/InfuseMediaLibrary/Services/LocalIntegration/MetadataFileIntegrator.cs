using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.LocalIntegration;

/// <summary>
/// Verantwortlich für die Integration von Metadaten-Dateien in die Infuse-Mediathek
/// </summary>
internal class MetadataFileIntegrator
{
    private readonly IFileOperations _fileOperations;
    private readonly ILogger<MetadataFileIntegrator> _logger;

    public MetadataFileIntegrator(IFileOperations fileOperations, ILogger<MetadataFileIntegrator> logger)
    {
        _fileOperations = fileOperations;
        _logger = logger;
    }

    internal async Task<Result> IntegrateMetadataFileAsync(MediaSetDirectory mediaSetDirectory, SupportedVideo integratedVideo)
    {
        // Wenn keine Metadaten-Datei vorhanden ist, gibt es nichts zu integrieren
        if (mediaSetDirectory.MetadataFile.HasNoValue)
        {
            _logger.LogInformation("Keine Metadaten-Datei vorhanden. Es gibt nichts zu integrieren.");
            return Result.Success();
        }

        // Ermittle das Zielverzeichnis für die Bild-Datei. Dieses ist das gleiche wie das Zielverzeichnis der Video-Datei, die in vorherigen Schritten integriert wurde.
        var videoTargetDirectory = integratedVideo.Directory;
        if (videoTargetDirectory == null)
        {
            return Result.Failure($"Das Verzeichnis der Video-Datei {integratedVideo} konnte nicht ermittelt werden. Das Verzeichnis wird benötigt, um die Bild-Dateien in das Infuse-Mediathek-Verzeichnis zu verschieben.");
        }

        // Ermittle den Ziel-Pfad für die Metadaten-Datei. Die XML-Datei hat den gleichen Namen wie die Videodatei, aber die Dateiendung .xml
        var metadataFile = mediaSetDirectory.MetadataFile.Value;
        var targetPath = Path.Combine(videoTargetDirectory.FullName, Path.GetFileNameWithoutExtension(integratedVideo.Name) + ".xml");

        // Kopiere die Metadaten-Datei in das Infuse-Mediathek-Verzeichnis und überschreibe die Datei, falls sie bereits existiert
        var copyResult = await _fileOperations.CopyFileAsync(metadataFile, targetPath, true, false);
        if (copyResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Kopieren der Metadaten-Datei in das Infuse-Mediathek-Verzeichnis: {copyResult.Error}");
        }

        _logger.LogInformation("Metadaten-Datei {MetadataFile} wurde erfolgreich in die Infuse-Mediathek integriert.", metadataFile);
        return Result.Success();
    }
}
