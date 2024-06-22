using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.Common;

namespace Kurmann.Videoschnitt.LocalFileSystem.Services.ImageProcessing.MacOS;

public class MacOSColorConversionService : IColorConversionService
{
    private readonly ExecuteCommandService _executeCommandService;
    private readonly ILogger<MacOSColorConversionService> _logger;

    public MacOSColorConversionService(ExecuteCommandService executeCommandService, ILogger<MacOSColorConversionService> logger)
    {
        _executeCommandService = executeCommandService;
        _logger = logger;
    }

    public async Task<Result> ConvertColorSpaceAsync(string inputFilePath, string outputFilePath, string inputColorSpace = "bt2020", string outputColorSpace = "adobe_rgb")
    {
        if (string.IsNullOrWhiteSpace(inputFilePath))
        {
            return Result.Failure("Ein Eingabepfad muss angegeben werden.");
        }

        if (!File.Exists(inputFilePath))
        {
            return Result.Failure($"Die Eingabedatei existiert nicht: {inputFilePath}");
        }

        _logger.LogInformation($"Wandle Farbraum von {inputColorSpace} nach {outputColorSpace} um: {inputFilePath}");

        var arguments = $"-m /System/Library/ColorSync/Profiles/AdobeRGB1998.icc \"{inputFilePath}\" --out \"{outputFilePath}\"";
        _logger.LogInformation($"sips-Befehl: sips {arguments}");
        var result = await _executeCommandService.ExecuteCommandAsync("sips", arguments);

        if (result.IsSuccess)
        {
            _logger.LogInformation($"Erfolgreiches Umwandeln des Farbraums von {inputColorSpace} nach {outputColorSpace}: {inputFilePath}");
            return Result.Success();
        }

        _logger.LogError($"Failed to convert artwork image to Adobe RGB colorspace: {inputFilePath}");
        return Result.Failure($"Fehler beim Umwandeln des Farbraums von {inputColorSpace} nach {outputColorSpace}: {result.Error}");
    }
}