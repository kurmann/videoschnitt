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
    /// Der Name des Mediensets. Entspricht dem Titel aller Videos (im Titel-Tag enthalten).
    /// </summary>
    public string Titel { get; set; } 

    /// <summary>
    /// Der Dateinamentitel Titel, der kompatibel ist als Dateiname. So werden gewisse Sonderzeichen, die nicht kompatibel sind für MacOS, Linux oder Windows, ersetzt.
    /// </summary>
    public string TitelAsFilename { get; set; }

    /// <summary>
    /// Der Name des Mediensets. Entspricht dem Aufnahmedatum als ISO-String und dem Dateinamentitel ohne Varianten-Suffix. Mit dem Mediensetnamen ist ein Medienset eindeutig identifizierbar.
    /// So kann ein Titel bspw. mehrmals vorkommen bspw. "Wanderung auf den Pilatus" aber unter verschiedenen Aufnahmedaten unterscheidbar.
    /// Beispiel: "2024-06-05 Wanderung auf den Pilatus" und "2018-07-12 Wanderung auf den Pilatus"
    /// </summary>
    public string MediaSetName { get; set; }
}
