namespace Kurmann.Videoschnitt.ConfigurationModule.Settings;

public class MediaSetOrganizerSettings
{
    public const string SectionName = "MediaSetOrganizer";

    public MediaSetSettings MediaSet { get; set; } = new MediaSetSettings();
}

public class MediaSetSettings
{
    /// <summary>
    /// Liste der Varianten-Suffixe, die in den Dateinamen von Videos vorkommen können, einschließlich des Trennzeichens, die für den Medienserver bestimmt sind.
    /// </summary>
    public List<string> VideoVersionSuffixesForMediaServer { get; set; } = new List<string> { "-4K60-Medienserver", "-4K30-Medienserver", "-1080p-Medienserver" };

    /// <summary>
    /// Liste der Varianten-Suffixe, die in den Dateinamen von Videos vorkommen können, einschließlich des Trennzeichens, die für das Internet bestimmt sind.
    /// </summary>
    public List<string> VideoVersionSuffixesForInternet { get; set; } = new List<string> { "-4K-Internet", "-1080p-Internet" };

    /// <summary>
    /// In welches Unterverzeichnis des Medienset-Verzeichnisses sollen die Dateien für den Medienserver kopiert oder verschoben werden.
    /// </summary>
    /// <value></value>
    public string MediaServerFilesSubDirectoryName { get; set; } = "Medienserver";

    /// <summary>
    /// In welches Unterverzeichnis des Medienset-Verzeichnisses sollen die Dateien für das Internet kopiert oder verschoben werden.
    /// </summary>
    /// <value></value>
    public string InternetFilesSubDirectoryName { get; set; } = "Internet";

    /// <summary>
    /// In welches Unterverzeichnis des Medienset-Verzeichnisses sollen die Titelbilder kopiert oder verschoben werden.
    /// </summary>
    public string ImageFilesSubDirectoryName { get; set; } = "Titelbilder";

    /// <summary>
    /// In welches Unterverzeichnis des Medienset-Verzeichnisses sollen die Masterdateien kopiert oder verschoben werden.
    /// </summary>
    public string MasterfileSubDirectoryName { get; set; } = "Masterdatei";

    /// <summary>
    /// Unterstützte Video-Dateiendungen, die in den Medienset-Verzeichnissen erwartet werden.
    /// Dies umfasst alle Varianten von MPEG-4-Dateien sowie QuickTime-Dateien.
    /// </summary>
    /// <value></value>
    public List<string> SupportedVideoExtensions { get; set; } = new List<string> { ".mp4", ".mov", ".m4v" };

    /// <summary>
    /// Definition der Suffixe für Portrait- und Landscape-Bilder.
    /// </summary>
    public OrientationSuffixDefinition OrientationSuffixes { get; set; } = new OrientationSuffixDefinition();
}

public record OrientationSuffixDefinition(string Portrait = "-Portrait", string Landscape = "-Landscape")
{
    public bool HasPortraitSuffix(string fileName) => fileName.Contains(Portrait);

    public bool HasLandscapeSuffix(string fileName) => fileName.Contains(Landscape);
}
