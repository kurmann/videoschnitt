using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

public class InfuseMetadataXmlService
{
    private readonly ILogger<InfuseMetadataXmlService> _logger;

    public InfuseMetadataXmlService(ILogger<InfuseMetadataXmlService> logger)
    {
        _logger = logger;
    }

    public List<FileInfo> TryGetInfuseMetadataXmlFiles(string sourceDirectoryPath)
    {
        if (string.IsNullOrWhiteSpace(sourceDirectoryPath))
        {
            _logger.LogError("Der Quellverzeichnispfad darf nicht leer sein.");
            return new List<FileInfo>();
        }

        var infuseMetadataXmlFiles = new List<FileInfo>();
        var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);
        var sourceFiles = sourceDirectory.GetFiles("*.xml", SearchOption.AllDirectories);
        foreach (var sourceFile in sourceFiles)
        {
            // Lies die erste Zeile der Datei
            var firstLine = File.ReadLines(sourceFile.FullName).FirstOrDefault();

            // Prüfe, ob die erste Zeile der Datei "<?xml version="1.0" encoding="utf-8" standalone="yes"?>" lautet
            if (firstLine != "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>")
            {
                _logger.LogWarning($"Die Datei {sourceFile.FullName} ist keine Infuse-Metadaten-XML-Datei.");
                _logger.LogInformation($"Die erste Zeile der Datei lautet: {firstLine}");
                _logger.LogInformation("Die Datei wird ignoriert.");
                continue;
            }

            // Lies die zweite Zeile der Datei aus
            var secondLine = File.ReadLines(sourceFile.FullName).Skip(1).FirstOrDefault();

            // Prüfe, ob die zweite Zeile vorhanden ist
            if (secondLine == null)
            {
                _logger.LogWarning($"Die Datei {sourceFile.FullName} ist keine Infuse-Metadaten-XML-Datei.");
                _logger.LogInformation("Die Datei enthält keine zweite Zeile.");
                _logger.LogInformation("Die Datei wird ignoriert.");
                continue;
            }

            // Prüfe, ob die zweite Zeile "media type="Other" enthält
            if (!secondLine.Contains("media type=\"Other\""))
            {
                _logger.LogWarning($"Die Datei {sourceFile.FullName} ist keine Infuse-Metadaten-XML-Datei.");
                _logger.LogInformation($"Die zweite Zeile der Datei lautet: {secondLine}");
                _logger.LogInformation("Die Datei wird ignoriert.");
                continue;
            }

            // Füge die Datei zur Liste der Infuse-Metadaten-XML-Dateien hinzu
            infuseMetadataXmlFiles.Add(sourceFile);
        }

        return infuseMetadataXmlFiles;
    }
}