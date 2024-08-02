using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.LocalIntegration;

/// <summary>
/// Verantwortlich für die Ermittlung des Zielverzeichnisses für die Integration anhand von Metadaten und Konfiguration.
/// </summary>
internal class TargetPathService
{
    private readonly VideoMetadataService _videoMetadataService;

    public TargetPathService(VideoMetadataService videoMetadataService)
    {
        _videoMetadataService = videoMetadataService;
    }

    /// <summary>
    /// Gibt das Unterverzeichnis für das Medienset zurück nach folgendem Schema:
    /// <Infuse-Mediathek-Verzeichnis>/<Album>/<Aufnahmedatum.JJJJ>/<Aufnahmedatum.JJJJ-MM-DD>/<Titel ohne ISO-Datum>.<Dateiendung>
    /// </summary>
    /// <param name="album"></param>
    /// <param name="recordingDate"></param>
    /// <returns></returns>
    internal async Task<Result<InfuseMediaSubDirectory>> GetSubDirectory(SupportedVideo? videoFile)
    {
        if (videoFile == null)
        {
            return Result.Failure<InfuseMediaSubDirectory>("Die Video-Datei ist leer.");
        }

        var album = await _videoMetadataService.GetAlbumAsync(videoFile);
        if (album.IsFailure)
        {
            return Result.Failure<InfuseMediaSubDirectory>($"Das Zielverzeichnis für die Video-Datei {videoFile} konnte aufgrund Fehler bei der Album-Ermittlung nicht ermittelt werden: {album.Error}");
        }

        if (string.IsNullOrWhiteSpace(album.Value))
        {
            return Result.Failure<InfuseMediaSubDirectory>($"Das Album ist leer.");
        }

        var mediaSetName = _videoMetadataService.GetMediaSetName(videoFile);
        if (mediaSetName.IsFailure)
        {
            return Result.Failure<InfuseMediaSubDirectory>($"Das Zielverzeichnis für die Video-Datei {videoFile} konnte aufgrund Fehler bei der Titel-Ermittlung nicht ermittelt werden: {mediaSetName.Error}");
        }

        var subDirectory = new InfuseMediaSubDirectory(album.Value, mediaSetName.Value.Date.Year, mediaSetName.Value);
        return subDirectory;
    }

    /// <summary>
    /// Retourniert den Ziel-Dateinamen für die Video-Datei.
    /// </summary>
    /// <param name="videoFile"></param>
    /// <returns></returns>
    internal Result<string> GetTargetFileName(FileInfo videoFile)
    {
        // Der Ziel-Dateiname entspricht dem Medienset-Titel (also dem Medienset-Namen ohne ISO-Datum) + Dateiendung
        var mediaSetName = _videoMetadataService.GetMediaSetName(videoFile);
        if (mediaSetName.IsFailure)
        {
            return Result.Failure<string>($"Der Ziel-Dateiname für die Video-Datei {videoFile} konnte aufgrund Fehler bei der Titel-Ermittlung nicht ermittelt werden: {mediaSetName.Error}");
        }

        return $"{mediaSetName.Value.Title}{videoFile.Extension}";
    }
}

internal record InfuseMediaSubDirectory(string AlbumDirectoryName, int RecordingYearDirectoryName, MediaSetName MediaSetName)
{
    public string SubDirectory => Path.Combine(AlbumDirectoryName, RecordingYearDirectoryName.ToString(), MediaSetName);

    public override string ToString()
    {
        return SubDirectory;
    }

    public static implicit operator string(InfuseMediaSubDirectory subDirectory)
    {
        return subDirectory.SubDirectory;
    }

    public Result<DirectoryInfo> ToFullPath(string directoryInfo)
    {
        try
        {
            return new DirectoryInfo(Path.Combine(directoryInfo, SubDirectory));
        }
        catch (Exception ex)
        {
            return Result.Failure<DirectoryInfo>($"Fehler beim Erstellen des vollständigen Pfades für das Unterverzeichnis {SubDirectory}: {ex.Message}");
        }
    }
}