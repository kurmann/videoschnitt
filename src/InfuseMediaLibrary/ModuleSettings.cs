namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public class ModuleSettings
{
    public const string SectionName = "InfuseMediaLibrary";

    /// <summary>
    /// Das Suffix des Dateinamens, das für die Banner-Datei verwendet wird für die Infuse-Mediathek als Titelbild.
    public string? BannerFilePostfix { get; set; } = "-fanart";

    /// <summary>
    /// Das Suffix des zwischenzeitlich konvertierten Bildes, das für die Infuse-Mediathek verwendet wird.
    /// </summary>
    /// <value></value>
    public string? SuffixForConvertedTempImage { get; set; } = "-adobe_rgb";
}
