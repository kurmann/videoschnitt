using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.Common.Services.Metadata;

public class SipsMetadataService
{
    private readonly ExecuteCommandService _executeCommandService;
    private readonly ILogger<SipsMetadataService> _logger;
    private readonly ApplicationSettings _applicationSettings;

    public SipsMetadataService(ExecuteCommandService executeCommandService, ILogger<SipsMetadataService> logger, IOptions<ApplicationSettings> applicationSettings)
    {
        _executeCommandService = executeCommandService;
        _logger = logger;
        _applicationSettings = applicationSettings.Value;
    }

    public async Task<Result<(int Width, int Height)>> GetImageDimensionsWithSipsAsync(string filePath)
    {
        var sipsCommand = _applicationSettings.ExternalTools.Sips.Path;
        var arguments = $"-g pixelWidth -g pixelHeight \"{filePath}\"";
        var result = await _executeCommandService.ExecuteCommandAsync(sipsCommand, arguments);

        if (result.IsSuccess)
        {
            var lines = result.Value;
            int width = 0;
            int height = 0;

            foreach (var line in lines)
            {
                if (line.Contains("pixelWidth:"))
                {
                    width = int.Parse(line.Split(':')[1].Trim());
                }
                if (line.Contains("pixelHeight:"))
                {
                    height = int.Parse(line.Split(':')[1].Trim());
                }
            }

            if (width > 0 && height > 0)
            {
                return Result.Success((width, height));
            }
            return Result.Failure<(int Width, int Height)>("Fehler beim Parsen der Dimensionen.");
        }

        _logger.LogError("Error retrieving dimensions for file '{filePath}' with sips: {Error}", filePath, result.Error);
        return Result.Failure<(int Width, int Height)>(result.Error);
    }
}
