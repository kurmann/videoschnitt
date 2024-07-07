namespace Kurmann.Videoschnitt.ConfigurationModule.Settings;

/// <summary>
/// Einstellungen, die für die ganze Anwendung gelten.
/// </summary>
public class ApplicationSettings
{
    public const string SectionName = "Application";

    /// <summary>
    /// Der Pfad zur Infuse-Mediathek.
    /// </summary>
    public string InfuseMediaLibraryPath { get; set; } = DefaultInfuseMediaLibraryPath;
    public const string InfuseMediaLibraryPathConfigKey = $"{SectionName}:InfuseMediaLibraryPath";
    public const string DefaultInfuseMediaLibraryPath = "~/Movies/Infuse Media Library";
    public bool IsDefaultInfuseMediaLibraryPath => InfuseMediaLibraryPath == DefaultInfuseMediaLibraryPath;

    /// <summary>
    /// Eingangsverzeichnis, in dem die zu verarbeitenden Dateien liegen.
    /// </summary>
    public string InputDirectory { get; set; } = DefaultInputDirectory;
    public const string InputDirectoryConfigKey = $"{SectionName}:InputDirectory";
    public const string DefaultInputDirectory = "~/Movies/Final Cut Export";
    public bool IsDefaultInputDirectory => InputDirectory == DefaultInputDirectory;

    /// <summary>
    /// Der Pfad zum lokalen Medienset-Verzeichnis.
    /// </summary>
    /// <value></value>
    public string MediaSetPathLocal { get; set; } = DefaultMediaSetPathLocal;
    public const string MediaSetPathLocalConfigKey = $"{SectionName}:MediaSetPathLocal";
    public const string DefaultMediaSetPathLocal = "~/Movies/MediaSets";
    public bool IsDefaultMediaSetPathLocal => MediaSetPathLocal == DefaultMediaSetPathLocal;

    /// <summary>
    /// Der Pfad zum Medienset-Verzeichnis auf dem externen Archiv oder Medienserver.
    /// </summary>
    /// <value></value>
    public string? MediaSetPathRemote { get; set; }
    public const string MediaSetPathRemoteConfigKey = $"{SectionName}:MediaSetPathRemote";

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