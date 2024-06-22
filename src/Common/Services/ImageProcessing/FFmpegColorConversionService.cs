using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common;

namespace Kurmann.Videoschnitt.Common.Services.ImageProcessing;

/// <summary>
/// Implementiert die Konvertierung von Farbräumen mit Hilfe von FFmpeg.
/// </summary>
public class FFmpegColorConversionService : IColorConversionService
{
    private readonly ExecuteCommandService _executeCommandService;
    private readonly ILogger<FFmpegColorConversionService> _logger;

    public FFmpegColorConversionService(ExecuteCommandService executeCommandService, ILogger<FFmpegColorConversionService> logger)
    {
        _executeCommandService = executeCommandService;
        _logger = logger;
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
        _logger.LogInformation($"FFmpeg-Befehl: ffmpeg {arguments}");
        var result = await _executeCommandService.ExecuteCommandAsync("ffmpeg", arguments);

        if (result.IsSuccess)
        {
            _logger.LogInformation($"Erfolgreiches Umwandeln des Farbraums von {inputColorSpace} nach {outputColorSpace}: {inputFilePath}");
            return Result.Success();
        }

        return Result.Failure($"Fehler beim Umwandeln des Farbraums von {inputColorSpace} nach {outputColorSpace}: {result.Error}");
    }
}