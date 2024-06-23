namespace Kurmann.Videoschnitt.Common;

/// <summary>
/// Einstellungen, die für die ganze Anwendung gelten.
public class ApplicationSettings
{
    public const string SectionName = "Application";

    /// <summary>
    /// Der Pfad zur Infuse-Mediathek.
    /// </summary>
    public string? InfuseMediaLibraryPath { get; set; }

    /// <summary>
    /// Eingangsverzeichnis, in dem die zu verarbeitenden Dateien liegen.
    /// </summary>
    public string? InputDirectory { get; set; }

    /// <summary>
    /// Die Einstellungen für externe Tools wie FFmpeg oder SIPS.
    /// </summary>
    /// <returns></returns>
    public ExternalToolsSettings ExternalTools { get; set; } = new ExternalToolsSettings();
}

public class ExternalToolsSettings
{
    /// <summary>
    /// Einstellungen für das FFmpeg-Tool.
    /// </summary>
    /// <returns></returns>
    public FFMmpegSettings FFMpeg { get; set; } = new FFMmpegSettings();

    /// <summary>
    /// Einstellungen für das FFProbe-Tool.
    /// </summary>
    /// <returns></returns>
    public FFProbeSettings FFProbe { get; set; } = new FFProbeSettings();

    /// <summary>
    /// Einstellungen für das SIPS-Tool auf MacOS.
    /// </summary>
    /// <returns></returns>
    public SipsSettings Sips { get; set; } = new SipsSettings();
}

public class FFMmpegSettings
{
    /// <summary>
    /// Der absolute Pfad zu FFMpeg.
    /// </summary>
    /// <value></value>
    public string? Path { get; set; } = "/usr/local/bin/ffmpeg";
}

public class FFProbeSettings
{
    public string? Path { get; set; } = "/opt/local/bin/ffprobe";
}

public class SipsSettings
{
    /// <summary>
    /// Der absolute Pfad zu SIPS.
    /// </summary>
    /// <value></value>
    public string? Path { get; set; } = "/usr/bin/sips";
}