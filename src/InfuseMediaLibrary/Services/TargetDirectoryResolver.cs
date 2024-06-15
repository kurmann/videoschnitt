using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Entities;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

public class TargetDirectoryResolver
{
    private readonly ILogger<TargetDirectoryResolver> _logger;
    private readonly ModuleSettings _settings;

    public TargetDirectoryResolver(IOptions<ModuleSettings> settings, ILogger<TargetDirectoryResolver> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Ermittelt das Zielverzeichnis anhand der übergebenen MediaSetFiles.
    /// Das Zielverzeichnis ist das Verzeichnis des ersten MediaSetFiles.
    /// </summary>
    public Result<DirectoryInfo> GetTargetDirectory(List<FileInfo>? mediaSetFiles)
    {
        if (mediaSetFiles == null || mediaSetFiles.Count == 0)
        {
            return Result.Failure<DirectoryInfo>("MediaSetFiles darf nicht null oder leer sein.");
        }

        var targetDirectory = mediaSetFiles.First().Directory;
        if (targetDirectory == null)
        {
            return Result.Failure<DirectoryInfo>("Das Verzeichnis des ersten MediaSetFiles konnte nicht ermittelt werden.");
        }

        return Result.Success(targetDirectory);
    }

    public Result<DirectoryInfo> ResolveTargetDirectory(List<FileInfo>? mediaSetFiles, string libraryPath)
    {
        if (mediaSetFiles == null || mediaSetFiles.Count == 0)
        {
            return Result.Failure<DirectoryInfo>("MediaSetFiles darf nicht null oder leer sein.");
        }

        // Filtere XML-Dateien aus den Medienset-Dateien unter Benutzung von StringComparison.OrdinalIgnoreCase
        var infuseMetadataXmlFiles = mediaSetFiles.Where(f => f.Extension.Equals(".xml", StringComparison.OrdinalIgnoreCase)).ToList();

        // Iteriere über XML-Dateien in den Medienset-Dateien und prüfe, ob eines davon ein valides Infuse-Metadaten-XML ist
        string? albumNameFromMetadata = null;
        DateOnly? recordingDate = null;
        foreach (var infuseMetadataXmlFile in infuseMetadataXmlFiles)
        {
            var infuseMetadataXmlContent = File.ReadAllText(infuseMetadataXmlFile.FullName);
            var infuseMetadataResult = CustomProductionInfuseMetadata.Create(infuseMetadataXmlContent);
            if (infuseMetadataResult.IsSuccess)
            {
                // Ermittle das Album aus den Infuse-Metadaten
                albumNameFromMetadata = infuseMetadataResult.Value.Album;

                // Ermittle das Aufnahmedatum aus den Infuse-Metadaten
                recordingDate = infuseMetadataResult.Value.Published;
                break;
            }
        }

        if (albumNameFromMetadata == null)
        {
            return Result.Failure<DirectoryInfo>("Kein Albumname in den Infuse-Metadaten gefunden.");
        }
        if (recordingDate == null)
        {
            return Result.Failure<DirectoryInfo>("Kein Aufnahmedatum in den Infuse-Metadaten gefunden.");
        }

        // Ermittle das Zielverzeichnis mit dem Format "<LibraryPath>/<AlbumNameFromMetadata>/YYYY/YYYY-MM-DD"
        // Das Datum ist als Aufnahmedatum aus den Infuse-Metadaten definiert
        var targetDirectory = Path.Combine(libraryPath, albumNameFromMetadata, recordingDate.Value.Year.ToString(), recordingDate.Value.ToString("yyyy-MM-dd"));

        // Definiere ein DirectoryInfo-Objekt für das Zielverzeichnis
        try
        {
            Directory.CreateDirectory(targetDirectory);
        }
        catch (Exception ex)
        {
            return Result.Failure<DirectoryInfo>($"Fehler beim Definieren des Zielverzeichnisses {targetDirectory}: {ex.Message}");
        }

        return Result.Success(new DirectoryInfo(targetDirectory));
    }
    
}