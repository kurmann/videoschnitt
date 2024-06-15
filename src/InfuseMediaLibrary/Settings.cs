namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public class Settings
{
    public const string SectionName = "InfuseMediaLibrary";

    /// <summary>
    /// Wurzelverzeichnis der Infuse-Mediathek.
    /// </summary>
    public string? LibraryPath { get; set; }

    /// <summary>
    /// Verzeichnis, in dem die Quelldateien liegen, die in die Infuse-Mediathek importiert werden sollen.
    /// </summary>
    public string? SourceDirectoryPath { get; set; }
}
