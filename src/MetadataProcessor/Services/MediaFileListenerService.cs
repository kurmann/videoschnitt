using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

/// <summary>
/// Verantwortlich für das Auflisten von unterstützten Medien-Dateien eines Verzeichnisses.
/// Unterstützte Mediendateien sind Quicktime-Dateien (mov) und MP4-Dateien sowie JPG- und PNG-Dateien, die als Thumbnails verwendet werden können.
public class MediaFileListenerService
{
    private readonly ILogger<MediaFileListenerService> _logger;
    private readonly ApplicationSettings _settings;

    public MediaFileListenerService(ILogger<MediaFileListenerService> logger, IOptions<ApplicationSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    internal Result<List<FileInfo>> GetSupportedMediaFiles()
    {
        _logger.LogInformation("Unterstützte Medien-Dateien werden gesucht.");

        // Interpretiere den Pfad als Verzeichnis
        DirectoryInfo inputDirectory;
        try
        {
            // Prüfe ob ein Verzeichnis für die Medien-Dateien konfiguriert wurde
            if (_settings.InputDirectory == null)
            {
                return Result.Failure<List<FileInfo>>("Kein Eingangsverzeichnis konfiguriert.");
            }
            var inputDirectoryOrNull = new DirectoryInfo(_settings.InputDirectory);
            if (inputDirectoryOrNull == null)
            {
                return Result.Failure<List<FileInfo>>("Ungültiges Eingangsverzeichnis.");
            }
            inputDirectory = inputDirectoryOrNull;
        }
        catch (Exception ex)
        {
            return Result.Failure<List<FileInfo>>("Fehler beim Interpretieren des Eingangsverzeichnis: " + ex.Message);
        }

        // Prüfe ob das Verzeichnis existiert
        if (!inputDirectory.Exists)
        {
            return Result.Failure<List<FileInfo>>($"Das Eingangsverzeichnis {_settings.InputDirectory} existiert nicht.");
        }

        // Suche nach Quicktime-Dateien (mov) und MP4-Dateien sowie JPG- und PNG-Dateien
        var files = inputDirectory.EnumerateFiles("*", SearchOption.AllDirectories)
            .Where(file => file.Extension.Equals(".mov", StringComparison.OrdinalIgnoreCase) ||
                            file.Extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase) ||
                            file.Extension.Equals(".m4v", StringComparison.OrdinalIgnoreCase) ||
                            file.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            file.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase)).ToList();

        _logger.LogInformation("Es wurden {files.Count} unterstützte Medien-Dateien gefunden.", files.Count);
        return Result.Success(files);
    }
}
