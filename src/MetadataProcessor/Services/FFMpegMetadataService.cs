using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.CommonServices;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

public class FFmpegMetadataService
{
    private readonly ExecuteCommandService _executeCommandService;
    private readonly ILogger<FFmpegMetadataService> _logger;

    public FFmpegMetadataService(ExecuteCommandService executeCommandService, ILogger<FFmpegMetadataService> logger)
    {
        _executeCommandService = executeCommandService;
        _logger = logger;
    }

    public async Task<Result<string>> GetRawMetadataAsync(string filePath)
    {
        var arguments = $"-i \"{filePath}\" -f ffmetadata -";
        var result = await _executeCommandService.ExecuteCommandAsync("ffmpeg", arguments);

        if (result.IsSuccess)
        {
            var rawMetadata = string.Join("\n", result.Value);
            return Result.Success(rawMetadata);
        }

        _logger.LogError($"Error retrieving FFmpeg metadata: {result.Error}");
        return Result.Failure<string>(result.Error);
    }
}
