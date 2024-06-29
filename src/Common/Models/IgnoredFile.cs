namespace Kurmann.Videoschnitt.Common.Models;

/// <summary>
/// Repäsentiert eine ignorierte Datei und den Grund, warum sie ignoriert wird.
/// </summary>
/// <param name="FileInfo"></param>
/// <param name="Reason"></param>
/// <returns></returns>
public record IgnoredFile(FileInfo FileInfo, IgnoredFileReason Reason = IgnoredFileReason.NotDefined);

/// <summary>
/// Gibt an, warum eine Datei ignoriert wird.
/// </summary>
public enum IgnoredFileReason
{
    /// <summary>
    /// Der Grund, warum die Datei ignoriert wird, ist nicht definiert.
    /// </summary>
    NotDefined,

    /// <summary>
    /// Die Datei wird ignoriert, weil sie nicht unterstützt wird.
    /// </summary>
    NotSupported,

    /// <summary>
    /// Die Datei wird ignoriert, weil sie aktuell in Verwendung ist.
    /// </summary>
    FileInUse,

    /// <summary>
    /// Die Datei wird ignoriert, weil sie sich in einem Unterverzeichnis befindet.
    /// </summary>
    LocatedInSubDirectory,

    /// <summary>
    /// Die Datei wird ignoriert, weil sie versteckt ist.
    /// </summary>
    Hidden,

    /// <summary>
    /// Die Datei wird ignoriert, weil sie ein Verzeichnis ist.
    /// </summary>
    Directory
}