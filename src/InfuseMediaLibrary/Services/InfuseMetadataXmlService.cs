using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Entities;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

public class InfuseMetadataXmlService
{
    private readonly ILogger<InfuseMetadataXmlService> _logger;

    public InfuseMetadataXmlService(ILogger<InfuseMetadataXmlService> logger) => _logger = logger;

    public Result<List<FileInfo>> GetInfuseMetadataXmlFiles(string? directoryPath)
    {
        // Prüfe, ob ein Verzeichnis angegeben wurde
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return Result.Failure<List<FileInfo>>("Kein Verzeichnis angegeben.");
        }

        // Parse den Verzeichnispfad
        var directoryResult = ParseDirectoryPath(directoryPath);
        if (directoryResult.IsFailure)
        {
            return Result.Failure<List<FileInfo>>(directoryResult.Error);
        }

        // Ermittle alle XML-Dateien im Verzeichnis
        var directory = directoryResult.Value;
        var xmlFiles = directory.EnumerateFiles("*.xml", SearchOption.TopDirectoryOnly)
            .Where(file => file.Extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Iteriere über XML-Dateien in den Medienset-Dateien und gib alle validen Infuse-Metadaten-XML-Dateien zurück
        var infuseMetadataXmlFiles = new List<FileInfo>();
        foreach (var infuseMetadataXmlFile in xmlFiles)
        {
            var infuseMetadataXmlContent = File.ReadAllText(infuseMetadataXmlFile.FullName);
            var infuseMetadataResult = CustomProductionInfuseMetadata.Create(infuseMetadataXmlContent);
            if (infuseMetadataResult.IsSuccess)
            {
                infuseMetadataXmlFiles.Add(infuseMetadataXmlFile);
            }
            else
            {
                // Informiere, dass die XML-Datei nicht als Infuse-Metadaten-XML-Datei erkannt wurde
                _logger.LogWarning($"Die XML-Datei {infuseMetadataXmlFile.FullName} konnte nicht als Infuse-Metadaten-XML-Datei erkannt werden. XML-Datei wird ignoriert.");
            }
        }

        return infuseMetadataXmlFiles;
    }

    private Result<DirectoryInfo> ParseDirectoryPath(string directoryPath)
    {
        try
        {
            return new DirectoryInfo(directoryPath);
        }
        catch (Exception ex)
        {
            return Result.Failure<DirectoryInfo>($"Fehler beim Parsen des Verzeichnispfads: {ex.Message}");
        }
    }
}