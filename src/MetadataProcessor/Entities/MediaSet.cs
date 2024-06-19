using CSharpFunctionalExtensions;

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
    /// <summary>
    /// Der Name des Mediensets. Entspricht dem Titel aller Videos (im Titel-Tag enthalten) im Medienset. Muss also für jedes Video im Medienset identisch sein.
    /// Beispiel: "Wanderung auf den Pilatus" oder "Besteigung des Matterhorns: Vorbereitungsphase"
    /// </summary>
    public string Titel { get; set; } 

    /// <summary>
    /// Der Dateinamentitel Titel, der kompatibel ist als Dateiname. So werden gewisse Sonderzeichen, die nicht kompatibel sind für MacOS, Linux oder Windows, ersetzt.
    /// In den meisten Fällen entspricht der Dateinamentitel dem Titel.
    /// Beispiel: "Wanderung auf den Pilatus" bleibt "Wanderung auf den Pilatus". 
    /// Beispiel: "Besteigung des Matterhorns: Vorbereitungsphase" wird zu "Besteigung des Matterhorns - Vorbereitungsphase"
    /// </summary>
    public string TitelAsFilename { get; set; }

    /// <summary>
    /// Der Name des Mediensets. Entspricht dem Aufnahmedatum als ISO-String und dem Dateinamentitel ohne Varianten-Suffix. Mit dem Mediensetnamen ist ein Medienset eindeutig identifizierbar.
    /// So kann ein Titel bspw. mehrmals vorkommen bspw. "Wanderung auf den Pilatus" aber unter verschiedenen Aufnahmedaten unterscheidbar.
    /// Beispiel: "2024-06-05 Wanderung auf den Pilatus" und "2018-07-12 Wanderung auf den Pilatus"
    /// </summary>
    public string MediaSetName { get; set; }

    /// <summary>
    /// Die Videodatei für den Medienserver. Darf nur ein Video enthalten.
    /// </summary>
    public Maybe<MediaserverFile> MediaserverFile { get; set; }
}

/// <summary>
/// Repräsentiert eine Videodatei für den Medienserver.
/// Kann entweder eine QuickTime-Movie-Datei oder eine Mpeg4-Datei sein.
/// </summary>
public class MediaServerFile
{
    public FileInfo FileInfo { get; set; }
    public MediaServerFileType FileType { get; set; }
}

public enum MediaServerFileType
{
    QuickTimeMovie,
    Mpeg4
}
