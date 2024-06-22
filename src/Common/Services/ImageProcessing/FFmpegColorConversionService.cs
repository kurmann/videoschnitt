using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Common.Services.ImageProcessing;

/// <summary>
/// Implementiert die Konvertierung von Farbräumen mit Hilfe von FFmpeg.
/// </summary>
public class FFmpegColorConversionService : IColorConversionService
{
    private readonly ExecuteCommandService _executeCommandService;
    private readonly ILogger<FFmpegColorConversionService> _logger;
    private string _ffmpegCommand;

    private const string DefaultFFmpegCommand = "ffmpeg";

    public FFmpegColorConversionService(ExecuteCommandService executeCommandService, ILogger<FFmpegColorConversionService> logger, IOptions<ApplicationSettings> applicationSettings)
    {
        _executeCommandService = executeCommandService;
        _logger = logger;
        
        // Prüfe ob ein vollständiger Pfad von FFmpeg übergeben wurde, ansonsten nimm an, dass die Umgebungsvariable gesetzt ist. Gib eine Warnung aus, wenn die Umgebungsvariable nicht gesetzt ist.
        if (applicationSettings.Value.ExternalTools?.FFMpeg?.Path == null)
            _logger.LogWarning("FFmpeg-Pfad nicht gesetzt. Es wird angenommen, dass FFmpeg in der Umgebungsvariable PATH gesetzt ist.");
        else
            _logger.LogInformation($"FFmpeg-Pfad: {applicationSettings.Value.ExternalTools?.FFMpeg?.Path}");

        _ffmpegCommand = applicationSettings.Value.ExternalTools?.FFMpeg?.Path ?? DefaultFFmpegCommand;
    }

    /// <summary>
    /// Konvertiert den Farbraum einer Bilddatei von einem Farbraum in einen anderen.
    /// </summary>
    /// <param name="inputFilePath"></param>
    /// <param name="outputFilePath"></param>
    /// <param name="inputColorSpace"></param>
    /// <param name="outputColorSpace"></param>
    /// <returns></returns>
    public async Task<Result> ConvertColorSpaceAsync(string inputFilePath, string outputFilePath, string inputColorSpace = "bt2020", string outputColorSpace = "adobe_rgb")
    {
        var arguments = $"-i \"{inputFilePath}\" -vf \"colorspace=all={inputColorSpace}:all={outputColorSpace}\" \"{outputFilePath}\"";
        _logger.LogInformation($"Wandle Farbraum von {inputColorSpace} nach {outputColorSpace} um: {inputFilePath}");
        _logger.LogInformation($"FFmpeg-Befehl: {_ffmpegCommand} {arguments}");
        var result = await _executeCommandService.ExecuteCommandAsync(_ffmpegCommand, arguments);

        if (result.IsSuccess)
        {
            _logger.LogInformation($"Erfolgreiches Umwandeln des Farbraums von {inputColorSpace} nach {outputColorSpace}: {inputFilePath}");
            return Result.Success();
        }

        return Result.Failure($"Fehler beim Umwandeln des Farbraums von {inputColorSpace} nach {outputColorSpace}: {result.Error}");
    }
}