namespace Kurmann.Videoschnitt.ConfigurationModule.Settings;

public class InfuseMediaLibrarySettings
{
    public const string SectionName = "InfuseMediaLibrary";

    /// <summary>
    /// Der Pfad zur lokalen Infuse-Mediathek. Dieser Pfad wird für die Integration der lokalen Mediathek in die Infuse-Mediathek verwendet.
    /// </summary>
    /// <returns></returns>
    public string InfuseMediaLibraryPathLocal { get; set; } = ExpandHomeDirectory(DefaultInfuseMediaLibraryPathLocal);
    public const string InfuseMediaLibraryPathLocalConfigKey = $"{SectionName}:InfuseMediaLibraryPathLocal";
    public const string DefaultInfuseMediaLibraryPathLocal = "~/Movies/Infuse Media Library";
    public bool IsDefaultInfuseMediaLibraryPathLocal => InfuseMediaLibraryPathLocal == DefaultInfuseMediaLibraryPathLocal;

    /// <summary>
    /// Der Pfad zur Infuse-Mediathek auf dem Medienserver (Netzwerkspeicher, bspw. NAS). Dieser Pfad wird für die Integration der lokalen Mediathek in die Infuse-Mediathek verwendet.
    /// Hinweis: Hier existiert keine Default-Konfiguration, da dieser Pfad in der Regel nicht auf dem lokalen Rechner existiert.
    /// </summary>
    /// <value></value>
    public string? InfuseMediaLibraryPathRemote { get; set; }
    public const string InfuseMediaLibraryPathRemoteConfigKey = $"{SectionName}:InfuseMediaLibraryPathRemote";

    public static string ExpandHomeDirectory(string path)
    {
        if (path.StartsWith('~'))
        {
            var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return path.Replace("~", homeDirectory);
        }
        return path;
    }

    /// <summary>
    /// Das Suffix des Dateinamens, das für die Banner-Datei verwendet wird für die Infuse-Mediathek als Titelbild.
    public string? BannerFilePostfix { get; set; } = "-fanart";

    /// <summary>
    /// Das Suffix des zwischenzeitlich konvertierten Bildes, das für die Infuse-Mediathek verwendet wird.
    /// </summary>
    /// <value></value>
    public string? SuffixForConvertedTempImage { get; set; } = "-adobe_rgb";

    /// <summary>
    /// Die bevorzugte Dateiendung für JPEG-Dateien.
    /// </summary>
    /// <value></value>
    public string? PreferredJpgExtension { get; set; } = ".jpg";
}
