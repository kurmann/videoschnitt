using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.ConfigurationModule.Services;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;

namespace Kurmann.Videoschnitt.Common.Services.Metadata;

/// <summary>
/// Verantwortlich für das Extrahieren von Metadaten aus Medien-Dateien mit FFmpeg.
/// </summary>
public class FFmpegMetadataService
{
    private readonly ExecuteCommandService _executeCommandService;
    private readonly ILogger<FFmpegMetadataService> _logger;
    private readonly string _ffmpegCommand;
    private readonly string _ffprobeCommand;
    private readonly ApplicationSettings _applicationSettings;

    private const string DefaultFFMpegCommand = "ffmpeg";
    private const string DefaultFFProbeCommand = "ffprobe";

    public FFmpegMetadataService(ExecuteCommandService executeCommandService, ILogger<FFmpegMetadataService> logger, IConfigurationService configurationService)
    {
        _executeCommandService = executeCommandService;
        _logger = logger;
        _applicationSettings = configurationService.GetSettings<ApplicationSettings>();

        // Warne, wenn die Umgebungsvariablen für FFmpeg nicht gesetzt sind.
        if (_applicationSettings.ExternalTools?.FFMpeg?.Path == null)
            _logger.LogWarning("FFmpeg-Pfad nicht gesetzt. Es wird angenommen, dass FFmpeg in der Umgebungsvariable PATH gesetzt ist.");
        else
            _logger.LogInformation("FFmpeg-Pfad: {Path}", _applicationSettings.ExternalTools.FFMpeg.Path);

        // Warne, wenn die Umgebungsvariablen für FFprobe nicht gesetzt sind.
        if (_applicationSettings.ExternalTools?.FFProbe?.Path == null)
            _logger.LogWarning("FFprobe-Pfad nicht gesetzt. Es wird angenommen, dass FFprobe in der Umgebungsvariable PATH gesetzt ist.");
        else
            _logger.LogInformation("FFprobe-Pfad: {Path}", _applicationSettings.ExternalTools.FFProbe.Path);

        // Setze die Pfade zu FFmpeg und FFprobe aus den Einstellungen oder verwende die Standardwerte.
        _ffmpegCommand = _applicationSettings.ExternalTools?.FFMpeg.Path ?? DefaultFFMpegCommand;
        _ffprobeCommand = _applicationSettings.ExternalTools?.FFProbe.Path ?? DefaultFFProbeCommand;
    }

    /// <summary>
    /// Gibt die Roh-Metadaten eines Medien-Files zurück im FFmpeg-Format.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task<Result<FFmpegMetadata>> GetRawMetadataAsync(string filePath)
    {
        var arguments = $"-i \"{filePath}\" -f ffmetadata -";
        var result = await _executeCommandService.ExecuteCommandAsync(_ffmpegCommand, arguments);

        if (result.IsSuccess)
        {
            // Jede Zeile besteht aus einem Key-Value-Paar, getrennt durch ein '='.
            var metadata = new FFmpegMetadata(result.Value);
            return Result.Success(metadata);
        }

        _logger.LogError("Error retrieving FFmpeg metadata: {Error}", result.Error);
        return Result.Failure<FFmpegMetadata>(result.Error);
    }

    public async Task<Result<string>> GetMetadataFieldAsync(FileInfo fileInfo, string field)
    {
        var arguments = $"-v error -show_entries format_tags={field} -of default=noprint_wrappers=1:nokey=1 \"{fileInfo.FullName}\"";
        var result = await _executeCommandService.ExecuteCommandAsync(_ffprobeCommand, arguments);

        if (result.IsSuccess)
        {
            var metadataValue = string.Join("\n", result.Value).Trim();
            return Result.Success(metadataValue);
        }

        _logger.LogError("Error retrieving FFprobe metadata field '{field}': {Error}", field, result.Error);
        return Result.Failure<string>(result.Error);
    }

    public async Task<Result<string>> GetVideoCodecNameAsync(string filePath)
    {
        var arguments = $"-v error -select_streams v:0 -show_entries stream=codec_name -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"";
        var result = await _executeCommandService.ExecuteCommandAsync(_ffprobeCommand, arguments);

        if (result.IsSuccess)
        {
            var codecName = string.Join("\n", result.Value).Trim();
            return Result.Success(codecName);
        }

        _logger.LogError("Error retrieving video codec for file '{filePath}': {Error}", filePath, result.Error);
        return Result.Failure<string>(result.Error);
    }

    public async Task<Result<string>> GetVideoCodecProfileAsync(string filePath)
    {
        var arguments = $"-v error -select_streams v:0 -show_entries stream=profile -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"";
        var result = await _executeCommandService.ExecuteCommandAsync(_ffprobeCommand, arguments);

        if (result.IsSuccess)
        {
            var codecProfile = string.Join("\n", result.Value).Trim();
            return Result.Success(codecProfile);
        }

        _logger.LogError("Error retrieving video codec profile for file '{filePath}': {Error}", filePath, result.Error);
        return Result.Failure<string>(result.Error);
    }
}

public record FFmpegMetadata(List<string> Metadata)
{
    /// <summary>
    /// Gibt die Metadaten als Zeichenfolge zurück.
    /// Jeder Listeneintrag kommt auf eine neue Zeile.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return string.Join("\n", Metadata);
    }

    public static implicit operator string(FFmpegMetadata metadata)
    {
        return metadata.ToString();
    }
}
