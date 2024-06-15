namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public class ModuleSettings
{
    public const string SectionName = "InfuseMediaLibrary";

    /// <summary>
    /// Das Suffix des Dateinamens, das für die Banner-Datei verwendet wird für die Infuse-Mediathek als Titelbild.
    public string? BannerFilePostfix { get; set; } = "-fanart";

    /// <summary>
    /// Die Varianten-Suffixe, die in den Dateinamen von Videos vorkommen können, einschließlich des Trennzeichens damit diese in die Infuse-Mediathek integriert werden.
    /// </summary>
    public List<string>? VideoVersionSuffixesToIntegrate { get; set; } = new List<string> { "-4K60-Medienserver", "-4K30-Medienserver", };
}
