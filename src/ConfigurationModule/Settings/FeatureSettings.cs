namespace Kurmann.Videoschnitt.ConfigurationModule.Settings;

public record FeatureSettings
{
    public InfuseMediaLibrarySettings InfuseMediaLibrarySettings { get; init; } = new InfuseMediaLibrarySettings();
    public MetadataProcessingSettings MetadataProcessingSettings { get; init; } = new MetadataProcessingSettings();
}
