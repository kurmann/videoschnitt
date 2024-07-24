using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.Integration;

/// <summary>
/// Verantwortlich für die Integration von Metadaten-Dateien in die Infuse-Mediathek
/// </summary>
internal class MetadataFileIntegrator
{
    private readonly InfuseMediaLibrarySettings _infuseMediaLibrarySettings;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;
    private readonly IFileOperations _fileOperations;

    public MetadataFileIntegrator(IOptions<InfuseMediaLibrarySettings> infuseMediaLibrarySettings, IOptions<MediaSetOrganizerSettings> mediaSetOrganizerSettings, IFileOperations fileOperations)
    {
        _infuseMediaLibrarySettings = infuseMediaLibrarySettings.Value;
        _mediaSetOrganizerSettings = mediaSetOrganizerSettings.Value;
        _fileOperations = fileOperations;
    }

    
}
