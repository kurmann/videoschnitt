using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.Integration;

/// <summary>
/// Verantwortlich für die Ermittlung des Zielverzeichnisses für die Integration anhand von Metadaten und Konfiguration.
/// </summary>
internal class TargetPathService
{
    private readonly ApplicationSettings _applicationSettings;
    private readonly VideoMetadataService _videoMetadataService;

    public TargetPathService(IOptions<ApplicationSettings> applicationSettings, VideoMetadataService videoMetadataService)
    {
        _applicationSettings = applicationSettings.Value;
        _videoMetadataService = videoMetadataService;
    }

    /// <summary>
    /// Gibt das Zielverzeichnis für das Medienset zurück nach folgendem Schema:
    /// <Infuse-Mediathek-Verzeichnis>/<Album>/<Aufnahmedatum.JJJJ>/<Aufnahmedatum.JJJJ-MM-DD>/<Titel ohne ISO-Datum>.<Dateiendung>
    /// </summary>
    /// <param name="album"></param>
    /// <param name="recordingDate"></param>
    /// <returns></returns>
    internal async Task<Result<DirectoryInfo>> GetTargetDirectoryAsync(FileInfo videoFile)
    {
        var album = await _videoMetadataService.GetAlbumAsync(videoFile);
        if (album.IsFailure)
        {
            return Result.Failure<DirectoryInfo>($"Das Zielverzeichnis für die Video-Datei {videoFile} konnte aufgrund Fehler bei der Album-Ermittlung nicht ermittelt werden: {album.Error}");
        }

        var title = _videoMetadataService.GetTitle(videoFile);
        if (title.IsFailure)
        {
            return Result.Failure<DirectoryInfo>($"Das Zielverzeichnis für die Video-Datei {videoFile} konnte aufgrund Fehler bei der Titel-Ermittlung nicht ermittelt werden: {title.Error}");
        }

        var recordingDate = _videoMetadataService.GetRecordingDate(title.Value);
        if (recordingDate.IsFailure)
        {
            return Result.Failure<DirectoryInfo>($"Das Zielverzeichnis für die Video-Datei {videoFile} konnte aufgrund Fehler bei der Aufnahmedatum-Ermittlung nicht ermittelt werden: {recordingDate.Error}");
        }

        if (string.IsNullOrWhiteSpace(album.Value))
        {
            return Result.Failure<DirectoryInfo>($"Das Album ist leer.");
        }
        
        if (string.IsNullOrWhiteSpace(title.Value))
        {
            return Result.Failure<DirectoryInfo>($"Der Titel ist leer.");
        }

        if (videoFile == null)
        {
            return Result.Failure<DirectoryInfo>($"Die Video-Datei ist null.");
        }

        var targetDirectory = Path.Combine(_applicationSettings.InfuseMediaLibraryPathLocal, album.Value, recordingDate.Value.Year.ToString(), title.Value);

        return new DirectoryInfo(targetDirectory);
    }
}
