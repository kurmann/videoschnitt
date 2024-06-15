namespace Kurmann.Videoschnitt.MetadataProcessor;

public class MetadataProcessorSettings
{
    public const string SectionName = "MetadataProcessing";

    public string? InputDirectory { get; set; } = "/media/inputDirectory";

    public MediaSetSettings? MediaSetSettings { get; set; } = new MediaSetSettings();
    public FileTypeSettings? FileTypeSettings { get; set; } = new FileTypeSettings();
}

public class MediaSetSettings
{
    /// <summary>
    /// Liste der Varianten-Suffixe, die in den Dateinamen von Videos vorkommen können, einschließlich des Trennzeichens.
    /// </summary>
    public List<string>? VideoVersionSuffixes { get; set; } = new List<string>{"-4K-Internet", "-4K60-Medienserver", "-4K30-Medienserver", "-1080p-Internet"};

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