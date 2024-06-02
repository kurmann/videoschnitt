namespace Kurmann.Videoschnitt.Features.MetadataProcessor;

public class Settings
{
    public const string SectionName = "MetadataProcessor";

    public List<string>? InputDirectories { get; set; }
    public string? OutputDirectory { get; set; }
}