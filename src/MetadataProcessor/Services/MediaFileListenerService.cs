using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wolverine;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

/// <summary>
/// Verantwortlich für das Auflisten von unterstützten Medien-Dateien eines Verzeichnisses.
/// Unterstützte Mediendateien sind Quicktime-Dateien (mov) und MP4-Dateien sowie JPG- und PNG-Dateien, die als Thumbnails verwendet werden können.
public class MediaFileListenerService(ILogger<MediaFileListenerService> logger, IMessageBus bus, IOptions<Settings> settings)
{
    private readonly ILogger<MediaFileListenerService> _logger = logger;
    private IMessageBus _bus = bus;
    private Settings _settings = settings.Value;

    internal Result<List<FileInfo>> GetSupportedMediaFiles()
    {
        // Interpretiere den Pfad als Verzeichnis
        DirectoryInfo inputDirectory;
        try
        {
            // Prüfe ob ein Verzeichnis für die Medien-Dateien konfiguriert wurde
            if (_settings.InputDirectory == null)
            {
                return Result.Failure<List<FileInfo>>("Kein Eingabeverzeichnis konfiguriert.");
            }
            var inputDirectoryOrNull = new DirectoryInfo(_settings.InputDirectory);
            if (inputDirectoryOrNull == null)
            {
                return Result.Failure<List<FileInfo>>("Ungültiges Eingabeverzeichnis.");
            }
            inputDirectory = inputDirectoryOrNull;
        }
        catch (Exception ex)
        {
            return Result.Failure<List<FileInfo>>("Fehler beim Interpretieren des Eingabeverzeichnisses: " + ex.Message);
        }

        // Prüfe ob das Verzeichnis existiert
        if (!inputDirectory.Exists)
        {
            return Result.Failure<List<FileInfo>>("Das Eingabeverzeichnis existiert nicht.");
        }

        // Suche nach Quicktime-Dateien (mov) und MP4-Dateien sowie JPG- und PNG-Dateien
        return inputDirectory.EnumerateFiles("*", SearchOption.AllDirectories)
            .Where(file => file.Extension.Equals(".mov", StringComparison.OrdinalIgnoreCase) ||
                            file.Extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase) ||
                            file.Extension.Equals(".m4v", StringComparison.OrdinalIgnoreCase) ||
                            file.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            file.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
