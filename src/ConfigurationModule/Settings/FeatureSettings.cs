namespace Kurmann.Videoschnitt.ConfigurationModule.Settings;

public record FeatureSettings
{
    public InfuseMediaLibrarySettings InfuseMediaLibrarySettings { get; init; } = new InfuseMediaLibrarySettings();
    public MediaSetOrganizerSettings MetadataProcessingSettings { get; init; } = new MediaSetOrganizerSettings();
}
