namespace Kurmann.Videoschnitt.MetadataProcessor;

/// <summary>
/// Einstellungen, die für die ganze Anwendung gelten.
/// </summary>
public class ApplicationSettings
{
    public const string SectionName = "Application";

    public const string DefaultInfuseMediaLibraryPath = "~/Movies/Infuse Media Library";
    public const string DefaultInputDirectory = "~/Movies/Final Cut Exporte";
    public bool IsDefaultInfuseMediaLibraryPath => InfuseMediaLibraryPath == DefaultInfuseMediaLibraryPath;
    public bool IsDefaultInputDirectory => InputDirectory == DefaultInputDirectory;

    /// <summary>
    /// Der Pfad zur Infuse-Mediathek.
    /// </summary>
    public string InfuseMediaLibraryPath { get; set; } = DefaultInfuseMediaLibraryPath;

    /// <summary>
    /// Eingangsverzeichnis, in dem die zu verarbeitenden Dateien liegen.
    /// </summary>
    public string InputDirectory { get; set; } = DefaultInputDirectory;
}