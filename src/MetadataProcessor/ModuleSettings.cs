namespace Kurmann.Videoschnitt.MetadataProcessor;

public class ModuleSettings
{
    public const string SectionName = "MetadataProcessing";

    public MediaSetSettings? MediaSet { get; set; } = new MediaSetSettings();
    public FileTypeSettings? FileTypes { get; set; } = new FileTypeSettings();
}

public class MediaSetSettings
{
    /// <summary>
    /// Liste der Varianten-Suffixe, die in den Dateinamen von Videos vorkommen können, einschließlich des Trennzeichens.
    /// </summary>
    public List<string>? VideoVersionSuffixes { get; set; } = new List<string>{"-4K-Internet", "-4K60-Medienserver", "-4K30-Medienserver", "-1080p-Internet"};

    /// <summary>
    /// Liste der Varianten-Suffixe, die in den Dateinamen von Videos vorkommen können, einschließlich des Trennzeichens, die für den Medienserver bestimmt sind.
    /// </summary>
    public List<string>? VideoVersionSuffixesForMediaServer { get; set; } = new List<string>{"-4K60-Medienserver", "-4K30-Medienserver", "-1080p-Medienserver"};

    /// <summary>
    /// Liste der Varianten-Suffixe, die in den Dateinamen von Videos vorkommen können, einschließlich des Trennzeichens, die für das Internet bestimmt sind.
    /// </summary>
    public List<string>? VideoVersionSuffixesForInternet { get; set; } = new List<string>{"-4K-Internet", "-1080p-Internet"};

    /// <summary>
    /// Liste der Varianten-Suffixe, die in den Dateinamen von Bilddateien vorkommen können, einschließlich des Trennzeichens.
    /// </summary>
    public List<string>? ImageVersionSuffixes { get; set; } = new List<string>{"", "-fanart"};
}

public class FileTypeSettings
{
    public List<string>? SupportedVideoExtensions { get; set; } = new List<string> { ".mov", ".mp4", ".m4v" };
    public List<string>? SupportedImageExtensions { get; set; } = new List<string> { ".jpg", ".jpeg", ".png" };
}