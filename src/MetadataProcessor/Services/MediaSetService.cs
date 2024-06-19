using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.MetadataProcessor.Entities;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

/// <summary>
/// Verantwortlich für das Erkennen des Mediensets.
/// </summary>
public class MediaSetService
{
    private readonly MediaSetVariantService _mediaSetVariantService;
    private readonly ModuleSettings _moduleSettings;
    private readonly ILogger<MediaSetService> _logger;

    public MediaSetService(MediaSetVariantService mediaSetVariantService, IOptions<ModuleSettings> moduleSettings, ILogger<MediaSetService> logger)
    {
        _mediaSetVariantService = mediaSetVariantService;
        _moduleSettings = moduleSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Analysiert das Verzeichnis und gruppiert die Medien-Dateien in Mediensets.
    public Result<List<MediaSet>> GroupToMediaSets(string directory)
    {
        // Prüfe ob ein Verzeichnis angegeben wurde
        if (string.IsNullOrWhiteSpace(directory))
        {
            return Result.Failure<List<MediaSet>>("Das Verzeichnis darf nicht leer sein.");
        }

        try 
        {
            return GroupToMediaSets(new DirectoryInfo(directory));
        }
        catch (Exception ex)
        {
            return Result.Failure<List<MediaSet>>($"Fehler beim Gruppieren der Medien-Dateien in Mediensets: {ex.Message}");
        }
    }

    public Result<List<MediaSet>> GroupToMediaSets(DirectoryInfo directoryInfo)
    {
        _logger.LogInformation("Versuche die Dateien im Verzeichnis in Medienset zu organisieren.");

        _logger.LogInformation("Prüfe ob das Verzeichnis existiert.");
        if (!directoryInfo.Exists)
        {
            return Result.Failure<List<MediaSet>>($"Das Verzeichnis {directoryInfo.FullName} existiert nicht.");
        }

        _logger.LogInformation("Prüfe ob das Verzeichnis Dateien enthält.");
        var files = directoryInfo.GetFiles();
        if (files.Length == 0)
        {
            return Result.Failure<List<MediaSet>>($"Das Verzeichnis {directoryInfo.FullName} enthält keine Dateien.");
        }

        _logger.LogInformation("Prüfe ob das Verzeichnis unterstützte Medien-Dateien enthält.");
        var videoVersionSuffixesForMediaServer = _moduleSettings.MediaSet?.VideoVersionSuffixesForMediaServer;
        if (videoVersionSuffixesForMediaServer == null || videoVersionSuffixesForMediaServer.Count == 0)
        {
            return Result.Failure<List<MediaSet>>("Keine Suffixe für die Medienserver-Versionen konfiguriert. Medienserver-Versionen können nicht erkannt werden.");
        }

        _logger.LogInformation("Prüfe ob das Verzeichnis unterstützte Medien-Dateien enthält.");
        var mediaServerFiles = files.Where(f => videoVersionSuffixesForMediaServer.Any(s => f.Name.Contains(s, StringComparison.InvariantCultureIgnoreCase))).ToList();

        _logger.LogInformation("Liste der Medienserver-Dateien:");
        foreach (var mediaServerFile in mediaServerFiles)
        {
            _logger.LogInformation(mediaServerFile.FullName);
        }

        if (mediaServerFiles.Count == 0)
        {
            return Result.Failure<List<MediaSet>>($"Keine Medienserver-Dateien gefunden. Es wurden keine Dateien mit den Suffixen {string.Join(", ", videoVersionSuffixesForMediaServer)} gefunden.");
        }

        _logger.LogInformation("Prüfe ob nur eine Medienserver-Datei gefunden wurde.");
        if (mediaServerFiles.Count > 1)
        {
            return Result.Failure<List<MediaSet>>($"Mehrere Medienserver-Dateien gefunden. Es sollte nur eine Datei mit den Suffixen {string.Join(", ", videoVersionSuffixesForMediaServer)} gefunden werden.");
        }

        _logger.LogInformation("Prüfe ob die Medienserver-Datei eine gültige Medienserver-Datei ist.");
        var mediaServerFileResult = MediaServerFile.Create(mediaServerFiles.First().FullName);
        if (mediaServerFileResult.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Die Medienserver-Datei ist keine gültige Medienserver-Datei: {mediaServerFileResult.Error}");
        }

        var mediaSet = MediaSet.Create("Test", "TestAsFileName", "TestAsFilePath", mediaServerFileResult.Value);
        if (mediaSet.IsFailure)
        {
            return Result.Failure<List<MediaSet>>($"Das Medienset konnte nicht erstellt werden: {mediaSet.Error}");
        }

        return Result.Success(new List<MediaSet> { mediaSet.Value });
    }
}