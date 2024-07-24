using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Services.Metadata;

namespace Kurmann.Videoschnitt.MediaSetOrganizer.Services.Metadata;

/// <summary>
/// Verantwortlich für das Erstellen von Infuse-XML-Dateien.
/// </summary>
public class InfuseXmlFileGenerator
{
    private readonly ILogger<InfuseXmlFileGenerator> _logger;
    private readonly FFprobeService _fFprobeService;

    public InfuseXmlFileGenerator(ILogger<InfuseXmlFileGenerator> logger, FFprobeService fFprobeService)
    {
        _logger = logger;
        _fFprobeService = fFprobeService;
    }

    /// <summary>
    /// Erstellt eine RAW-Metadatei für eine Videodatei.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task<Result<GenerateRawFileResponse>> GenerateRawFile(string filePath)
    {
        // Extrahiere die FFmpeg-Metadaten aus der Videodatei
        var ffprobeMetadata = await _fFprobeService.GetRawJsonMetadataAsync(filePath);
        if (ffprobeMetadata.IsFailure)
        {
            return Result.Failure<GenerateRawFileResponse>($"Fehler beim Extrahieren der FFprobe-Metadaten aus {filePath}: {ffprobeMetadata.Error}");
        }

        // Schreibe die FFprobe-Metadatei (mit dem gleichen Namen wie die Videodatei) als JSON-Datei
        var metadataFilePath = Path.ChangeExtension(filePath, ".json");
        await File.WriteAllTextAsync(metadataFilePath, ffprobeMetadata.Value);
        _logger.LogInformation("FFprobe-Metadaten-Datei für {filePath} erstellt: {jsonFilePath}", filePath, metadataFilePath);

        return new GenerateRawFileResponse(new FileInfo(metadataFilePath), ffprobeMetadata.Value);
    }

}

public record GenerateRawFileResponse(FileInfo MetadataFile, FFprobeMetadata Metadata);