namespace Kurmann.Videoschnitt.ConfigurationModule.Settings;

public class InfuseMediaLibrarySettings
{
    public const string SectionName = "InfuseMediaLibrary";

    public string InfuseMediaLibraryPathLocal { get; set; } = ExpandHomeDirectory(DefaultInfuseMediaLibraryPathLocal);
    public const string InfuseMediaLibraryPathLocalConfigKey = $"{SectionName}:InfuseMediaLibraryPathLocal";
    public const string DefaultInfuseMediaLibraryPathLocal = "~/Movies/Infuse Media Library";
    public bool IsDefaultInfuseMediaLibraryPathLocal => InfuseMediaLibraryPathLocal == DefaultInfuseMediaLibraryPathLocal;

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
