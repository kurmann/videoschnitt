using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Services.Metadata;
using Kurmann.Videoschnitt.PresentationAssetsBuilder.Entities;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder.Services;

/// <summary>
/// Verantwortlich für das Erstellen von Infuse-XML-Dateien.
/// </summary>
public class InfuseXmlFileGenerator
{
    private readonly ILogger<InfuseXmlFileGenerator> _logger;
    private readonly FFmpegMetadataService _ffmpegMetadataService;

    public InfuseXmlFileGenerator(ILogger<InfuseXmlFileGenerator> logger, FFmpegMetadataService ffmpegMetadataService)
    {
        _logger = logger;
        _ffmpegMetadataService = ffmpegMetadataService;
    }

    /// <summary>
    /// Erstellt eine Infuse-XML-Datei für eine Videodatei.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task<Result<FileInfo>> Generate(string filePath)
    {
        var metadataResult = await _ffmpegMetadataService.GetRawMetadataAsync(filePath);
        if (metadataResult.IsFailure)
        {
            return Result.Failure<FileInfo>($"Fehler beim Extrahieren der Metadaten aus {filePath}: {metadataResult.Error}");
        }

        // Parse die FFMpeg-Metadaten
        var ffmpegMetadata = FFmpegMetadata.Create(metadataResult.Value);
        if (ffmpegMetadata.IsFailure)
        {
            return Result.Failure<FileInfo>($"Fehler beim Parsen der extrahierten Metadaten aus {filePath}: {ffmpegMetadata.Error}");
        }

        // Erstelle ein Infuse-XML-Objekt aus den Metadaten
        var infuseXml = ffmpegMetadata.Value.ToInfuseXml();

        // Schreibe die Infuse-XML-Datei (mit dem gleichen Namen wie die Videodatei)
        var metadataFilePath = Path.ChangeExtension(filePath, ".xml");
        infuseXml.Save(metadataFilePath);

        return new FileInfo(metadataFilePath);
    }

}