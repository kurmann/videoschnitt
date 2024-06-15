namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

/// <summary>
/// Einstellungen, die für die ganze Anwendung gelten.
public class ApplicationSettings
{
    public const string SectionName = "Application";

    /// <summary>
    /// Der Pfad zur Infuse-Mediathek.
    /// </summary>
    public string? InfuseMediaLibraryPath { get; set; }

    /// <summary>
    /// Eingangsverzeichnis, in dem die zu verarbeitenden Dateien liegen.
    /// </summary>
    public string? InputDirectory { get; set; }
}