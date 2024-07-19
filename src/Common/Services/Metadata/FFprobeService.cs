using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.Common.Services.Metadata;

public class FFprobeService
{
    private readonly ExecuteCommandService _executeCommandService;
    private readonly ILogger<FFprobeService> _logger;
    private readonly ApplicationSettings _applicationSettings;
    private const string DefaultFFProbeCommand = "ffprobe";

    public FFprobeService(ExecuteCommandService executeCommandService, ILogger<FFprobeService> logger, IOptions<ApplicationSettings> applicationSettings)
    {
        _executeCommandService = executeCommandService;
        _logger = logger;
        _applicationSettings = applicationSettings.Value;
    }

    public async Task<Result<string>> GetRawJsonMetadataAsync(string filePath)
    {
        string ffprobeCommand = _applicationSettings.ExternalTools?.FFProbe?.Path ?? DefaultFFProbeCommand;
        string arguments = $"-v error -show_streams -show_format -print_format json \"{filePath}\"";

        var result = await _executeCommandService.ExecuteCommandAsync(ffprobeCommand, arguments);

        if (result.IsFailure)
        {
            _logger.LogError("Fehler beim Abrufen der Metadaten mit FFprobe: {Error}", result.Error);
            return Result.Failure<string>(result.Error);
        }

        // Füge die Ausgabezeilen zusammen auf jeweils eine Zeile
        var jsonRawData = string.Join(Environment.NewLine, result.Value);

        return Result.Success(jsonRawData);
    }
}
