using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder.Services;

/// <summary>
/// Verantwortlich für das Erstellen von Infuse-XML-Dateien aus allen Videodateien eines Verzeichnisses.
/// </summary>
public class InfuseXmlFileGenarator
{
    private readonly ILogger<InfuseXmlFileGenarator> _logger;

    public InfuseXmlFileGenarator(ILogger<InfuseXmlFileGenarator> logger)
    {
        _logger = logger;
    }

    public Task<Result<List<FileInfo>>> GenerateInfuseXmlFiles(string inputDirectory)
    {
        // Lese alle Verzeichnisse im Eingabeverzeichnis und nimm an, dass es sich um Mediensets handelt
        var mediaSetDirectoryInfos = new DirectoryInfo(inputDirectory).GetDirectories().ToList();
        _logger.LogInformation("Found {mediaSetDirectoryInfos.Count} media sets in {inputDirectory}", mediaSetDirectoryInfos.Count, inputDirectory);

        var infuseXmlFiles = new List<FileInfo>();
        return Task.FromResult(Result.Success(infuseXmlFiles));
    }
}
