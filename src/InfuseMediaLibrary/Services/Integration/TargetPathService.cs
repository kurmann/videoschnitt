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
    internal async Task<Result<DirectoryInfo>> GetTargetDirectoryAsync(FileInfo? videoFile)
    {
        if (videoFile == null)
        {
            return Result.Failure<DirectoryInfo>("Die Video-Datei ist leer.");
        }

        var album = await _videoMetadataService.GetAlbumAsync(videoFile);
        if (album.IsFailure)
        {
            return Result.Failure<DirectoryInfo>($"Das Zielverzeichnis für die Video-Datei {videoFile} konnte aufgrund Fehler bei der Album-Ermittlung nicht ermittelt werden: {album.Error}");
        }

        if (string.IsNullOrWhiteSpace(album.Value))
        {
            return Result.Failure<DirectoryInfo>($"Das Album ist leer.");
        }

        var mediaSetTitle = _videoMetadataService.GetMediaSetName(videoFile);
        if (mediaSetTitle.IsFailure)
        {
            return Result.Failure<DirectoryInfo>($"Das Zielverzeichnis für die Video-Datei {videoFile} konnte aufgrund Fehler bei der Titel-Ermittlung nicht ermittelt werden: {mediaSetTitle.Error}");
        }

        var targetDirectory = Path.Combine(_applicationSettings.InfuseMediaLibraryPathLocal, album.Value, mediaSetTitle.Value.Date.Year.ToString(), mediaSetTitle.Value);

        return new DirectoryInfo(targetDirectory);
    }

    internal Result<string> GetTargetFileName(FileInfo videoFile)
    {
        // Der Ziel-Dateiname ist ohne vorangestelltes ISO-Datum. Dieses muss also aus dem Titel entfernt werden.
        // var titleWithoutLeadingRecordingDate = title.Replace($"{recordingDate:yyyy-MM-dd} ", string.Empty);

        throw new NotImplementedException();
    }
}
