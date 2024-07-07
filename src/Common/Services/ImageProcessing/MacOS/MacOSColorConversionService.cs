using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.ConfigurationModule.Services;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.Common.Services.ImageProcessing.MacOS;

public class MacOSColorConversionService : IColorConversionService
{
    private readonly ExecuteCommandService _executeCommandService;
    private readonly ILogger<MacOSColorConversionService> _logger;
    
    private readonly string _sipsCommand;
    private const string DefaultSipsCommand = "sips";
    private const string DefaultOutputColorSpace = "adobe_rgb";
    private const string DefaultInputColorSpace = "bt2020";

    public MacOSColorConversionService(ExecuteCommandService executeCommandService, ILogger<MacOSColorConversionService> logger, IConfigurationService configurationService)
    {
        _executeCommandService = executeCommandService;
        _logger = logger;
        _sipsCommand = configurationService.GetSettings<ApplicationSettings>().ExternalTools?.Sips?.Path ?? DefaultSipsCommand;

        _executeCommandService = executeCommandService;
        _logger = logger;

        // Prüfe ob ein vollständiger Pfad von SIPS übergeben wurde, ansonsten nimm an, dass die Umgebungsvariable gesetzt ist. Gib eine Warnung aus, wenn die Umgebungsvariable nicht gesetzt ist.
        if (_sipsCommand == null)
            _logger.LogWarning("SIPS-Pfad nicht gesetzt. Es wird angenommen, dass SIPS in der Umgebungsvariable PATH gesetzt ist.");
        else
            _logger.LogInformation("SIPS-Pfad: {Path}", _sipsCommand);
        _sipsCommand ??= DefaultSipsCommand;
    }

    public async Task<Result> ConvertColorSpaceAsync(string inputFilePath, string outputFilePath, string inputColorSpace = DefaultInputColorSpace, string outputColorSpace = DefaultOutputColorSpace)
    {
        _logger.LogInformation("Konvertiere Farbraum von {inputColorSpace} nach {outputColorSpace} für: {inputFilePath}", inputColorSpace, outputColorSpace, inputFilePath);
        _logger.LogInformation("Hinweis: Der Eingangsfarbraum wird ignoriert, da sips diesen nicht benötigt um den Farbraum zu konvertieren.");

        if (outputColorSpace != DefaultOutputColorSpace)
        {
            return Result.Failure($"Der Ausgangsfarbraum muss '{DefaultOutputColorSpace}' sein, da sips der MacOSColorConversionService nur diese Umwandlung unterstützt.");
        }

        if (string.IsNullOrWhiteSpace(inputFilePath))
        {
            return Result.Failure("Ein Eingabepfad muss angegeben werden.");
        }

        if (!File.Exists(inputFilePath))
        {
            return Result.Failure($"Die Eingabedatei existiert nicht: {inputFilePath}");
        }

        _logger.LogInformation("Wandle Farbraum von {inputColorSpace} nach {outputColorSpace} um: {inputFilePath}", inputColorSpace, outputColorSpace, inputFilePath);

        var arguments = $"-m /System/Library/ColorSync/Profiles/AdobeRGB1998.icc \"{inputFilePath}\" --out \"{outputFilePath}\"";
        _logger.LogInformation("SIPS-Befehl: {_sipsCommand} {arguments}", _sipsCommand, arguments);
        var result = await _executeCommandService.ExecuteCommandAsync(_sipsCommand, arguments);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Erfolgreiches Umwandeln des Farbraums von {inputColorSpace} nach {outputColorSpace}: {inputFilePath}", inputColorSpace, outputColorSpace, inputFilePath);
            return Result.Success();
        }

        _logger.LogError("Failed to convert artwork image to Adobe RGB colorspace: {inputFilePath}", inputFilePath);
        return Result.Failure($"Fehler beim Umwandeln des Farbraums von {inputColorSpace} nach {outputColorSpace}: {result.Error}");
    }
}