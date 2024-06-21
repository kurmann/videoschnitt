namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public class ModuleSettings
{
    public const string SectionName = "InfuseMediaLibrary";

    /// <summary>
    /// Das Suffix des Dateinamens, das für die Banner-Datei verwendet wird für die Infuse-Mediathek als Titelbild.
    public string? BannerFilePostfix { get; set; } = "-fanart";
}
