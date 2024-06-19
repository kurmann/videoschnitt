using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;

namespace Kurmann.Videoschnitt.MetadataProcessor.Entities;

/// <summary>
/// Ein einzelnes Medienset ist eine Gruppe von Medien-Dateien, die zusammengehören.
/// So kann von einem Video mehrere Videoversionen existieren für verschiedene Zwecke (Medienserver und Cloud) und in verschiedenen Auflösungen
/// für verschiedene Datenübertragungsraten (4K, 1080p)
/// Ein Medienset wird erkannt durch folgendes Format im Dateinamen: "AufnahmedatumIso Titel(-Variante)".
/// Die Variante ist optional und wird durch einen Bindestrich getrennt.
/// </summary>
/// <remarks>
/// Beispiel für ein Medienset
/// 2024-06-05 Zeitraffer Wolken Testaufnahme-4K-Internet.m4v
/// 2024-06-05 Zeitraffer Wolken Testaufnahme-4K-Medienserver.mov
/// 2024-06-05 Zeitraffer Wolken Testaufnahme-1080p-Internet.m4v
/// 2024-06-05 Zeitraffer Wolken Testaufnahme.jpg
/// </remarks>
public class MediaSet 
{
    // <summary>
    /// Der Name des Mediensets. Entspricht dem Titel aller Videos (im Titel-Tag enthalten) im Medienset. Muss also für jedes Video im Medienset identisch sein.
    /// Beispiel: "Wanderung auf den Pilatus" oder "Besteigung des Matterhorns: Vorbereitungsphase"
    /// </summary>
    public string Title { get; } 

    /// <summary>
    /// Der Dateinamentitel Titel, der kompatibel ist als Dateiname. So werden gewisse Sonderzeichen, die nicht kompatibel sind für MacOS, Linux oder Windows, ersetzt.
    /// In den meisten Fällen entspricht der Dateinamentitel dem Titel.
    /// Beispiel: "Wanderung auf den Pilatus" bleibt "Wanderung auf den Pilatus". 
    /// Beispiel: "Besteigung des Matterhorns: Vorbereitungsphase" wird zu "Besteigung des Matterhorns - Vorbereitungsphase"
    /// </summary>
    public string TitleAsFilename { get; }

    /// <summary>
    /// Der Name des Mediensets. Entspricht dem Aufnahmedatum als ISO-String und dem Dateinamentitel ohne Varianten-Suffix. Mit dem Mediensetnamen ist ein Medienset eindeutig identifizierbar.
    /// So kann ein Titel bspw. mehrmals vorkommen bspw. "Wanderung auf den Pilatus" aber unter verschiedenen Aufnahmedaten unterscheidbar.
    /// Beispiel: "2024-06-05 Wanderung auf den Pilatus" und "2018-07-12 Wanderung auf den Pilatus"
    /// </summary>
    public string MediaSetName { get; }

    /// <summary>
    /// Die Videodatei für den Medienserver. Darf nur ein Video enthalten.
    /// </summary>
    public Maybe<MediaServerFile> MediaserverFile { get; }

    private MediaSet(string title, string titleAsFilename, string mediaSetName, Maybe<MediaServerFile> mediaserverFile)
    {
        Title = title;
        TitleAsFilename = titleAsFilename;
        MediaSetName = mediaSetName;
        MediaserverFile = mediaserverFile;
    }

    public static Result<MediaSet> Create(string title, string titleAsFilename, string mediaSetName, MediaServerFile? mediaserverFile)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<MediaSet>("Der Titel darf nicht leer sein.");
        }

        if (string.IsNullOrWhiteSpace(titleAsFilename))
        {
            return Result.Failure<MediaSet>("Der Dateinamentitel darf nicht leer sein.");
        }

        if (string.IsNullOrWhiteSpace(mediaSetName))
        {
            return Result.Failure<MediaSet>("Der Mediensetname darf nicht leer sein.");
        }

        return Result.Success(new MediaSet(title, titleAsFilename, mediaSetName, mediaserverFile ?? Maybe<MediaServerFile>.None));
    }
}

/// <summary>
/// Repräsentiert eine Videodatei für den Medienserver.
/// Kann entweder eine QuickTime-Movie-Datei oder eine Mpeg4-Datei sein.
/// </summary>
public class MediaServerFile
{
    public FileInfo FileInfo { get; }
    public MediaServerFileType FileType { get; }

    private MediaServerFile(FileInfo fileInfo, MediaServerFileType fileType)
    {
        FileInfo = fileInfo;
        FileType = fileType;
    }

    public static Result<MediaServerFile> Create(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Result.Failure<MediaServerFile>("Der Dateipfad darf nicht leer sein.");
        }
        try
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                return Result.Failure<MediaServerFile>($"Die Datei {filePath} existiert nicht.");
            }

            var quickTimeMovie = QuickTimeMovie.Create(fileInfo);
            if (quickTimeMovie.IsSuccess)
            {
                return new MediaServerFile(fileInfo, MediaServerFileType.QuickTimeMovie);
            }

            var mpeg4Video = Mpeg4Video.Create(fileInfo);
            if (mpeg4Video.IsSuccess)
            {
                return new MediaServerFile(fileInfo, MediaServerFileType.Mpeg4);
            }

            return Result.Failure<MediaServerFile>($"Die Datei {filePath} ist weder eine QuickTime-Movie-Datei noch eine Mpeg4-Datei.");
        }
        catch (Exception ex)
        {
            return Result.Failure<MediaServerFile>($"Fehler beim Einlesen der Pfads für die Medienserverdatei: {ex.Message}");
        }

    }
}

public enum MediaServerFileType
{
    QuickTimeMovie,
    Mpeg4
}
