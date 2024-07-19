using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Services.Metadata;
using System.Text.Json;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder.Services;

/// <summary>
/// Verantwortlich für das Erstellen von Infuse-XML-Dateien.
/// </summary>
public class InfuseXmlFileGenerator
{
    private readonly ILogger<InfuseXmlFileGenerator> _logger;
    private readonly FFmpegMetadataService _ffmpegMetadataService;
    private readonly FFprobeService _fFprobeService;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public InfuseXmlFileGenerator(ILogger<InfuseXmlFileGenerator> logger,
                                  FFmpegMetadataService ffmpegMetadataService,
                                  FFprobeService fFprobeService)
    {
        _logger = logger;
        _ffmpegMetadataService = ffmpegMetadataService;
        _fFprobeService = fFprobeService;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    /// <summary>
    /// Erstellt eine RAW-Metadatei für eine Videodatei.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task<Result<GenerateRawFileResponse>> GenerateRawFile(string filePath)
    {
        var metadataResult = await _ffmpegMetadataService.GetRawMetadataAsync(filePath);
        if (metadataResult.IsFailure)
        {
            return Result.Failure<GenerateRawFileResponse>($"Fehler beim Extrahieren der Metadaten aus {filePath}: {metadataResult.Error}");
        }

        // Schreibe die RAW-Metadatei (mit dem gleichen Namen wie die Videodatei) als Textdatei
        var metadataFilePath = Path.ChangeExtension(filePath, ".txt");
        await File.WriteAllTextAsync(metadataFilePath, metadataResult.Value);
        _logger.LogInformation("RAW-Metadaten-Datei für {filePath} erstellt: {metadataFilePath}", filePath, metadataFilePath);

        var ffprobeMetadata = await _fFprobeService.GetRawJsonMetadataAsync(filePath);
        if (ffprobeMetadata.IsFailure)
        {
            return Result.Failure<GenerateRawFileResponse>($"Fehler beim Extrahieren der FFprobe-Metadaten aus {filePath}: {ffprobeMetadata.Error}");
        }

        // Schreibe die FFprobe-Metadatei (mit dem gleichen Namen wie die Videodatei) als JSON-Datei
        var jsonFilePath = Path.ChangeExtension(filePath, ".json");
        await File.WriteAllTextAsync(jsonFilePath, ffprobeMetadata.Value);
        _logger.LogInformation("FFprobe-Metadaten-Datei für {filePath} erstellt: {jsonFilePath}", filePath, jsonFilePath);

        return new GenerateRawFileResponse(new FileInfo(metadataFilePath), metadataResult.Value);
    }

}

public record GenerateRawFileResponse(FileInfo MetadataFile, FFmpegMetadata Metadata);