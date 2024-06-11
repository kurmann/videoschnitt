namespace Kurmann.Videoschnitt.MetadataProcessor;

public class MetadataProcessorSettings
{
    public string? InputDirectory { get; set; }

    public MediaSetSettings? MediaSetSettings { get; set; } = new MediaSetSettings();

    public FileTypeSettings? FileTypeSettings { get; set; } = new FileTypeSettings();
}

public class MediaSetSettings
{
    /// <summary>
    /// Liste der Varianten-Suffixe, die in den Dateinamen von Videos vorkommen können, einschließlich des Trennzeichens.
    /// </summary>
    public List<string>? VideoVersionSuffixes { get; set; } = ["-4K-Internet", "-4K-Medienserver", "-1080p-Internet"];

    /// <summary>
    /// Liste der Varianten-Suffixe, die in den Dateinamen von Bilddateien vorkommen können, einschließlich des Trennzeichens.
    /// </summary>
    public List<string>? ImageVersionSuffixes { get; set; } = ["", "-fanart",];
}

public class FileTypeSettings
{
    public List<string>? SupportedVideoExtensions { get; set; } = [".mov", ".mp4", ".m4v"];
    public List<string>? SupportedImageExtensions { get; set; } = [".jpg", ".jpeg", ".png"];
}