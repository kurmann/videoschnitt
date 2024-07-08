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
}
