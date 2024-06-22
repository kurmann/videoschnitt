using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common;

namespace Kurmann.Videoschnitt.LocalFileSystem.Services.ImageProcessing;

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
        var result = await _executeCommandService.ExecuteCommandAsync("ffmpeg", arguments);

        if (result.IsSuccess)
        {
            return Result.Success();
        }

        _logger.LogError($"Error converting color space from {inputColorSpace} to {outputColorSpace}: {result.Error}");
        return Result.Failure(result.Error);
    }
}