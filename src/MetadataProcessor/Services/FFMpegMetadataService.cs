using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

public class FFmpegMetadataService
{
    private readonly CommandExecutorService _executorService;
    private readonly ILogger<FFmpegMetadataService> _logger;

    public FFmpegMetadataService(CommandExecutorService executorService, ILogger<FFmpegMetadataService> logger)
    {
        _executorService = executorService;
        _logger = logger;
    }

    public async Task<Result<string>> GetFFmpegMetadataAsync(string filePath)
    {
        var arguments = $"-i \"{filePath}\" -f ffmetadata -";
        var result = await _executorService.ExecuteCommandAsync("ffmpeg", arguments);

        if (result.IsSuccess)
        {
            var rawMetadata = string.Join("\n", result.Value);
            return Result.Success(rawMetadata);
        }

        _logger.LogError($"Error retrieving FFmpeg metadata: {result.Error}");
        return Result.Failure<string>(result.Error);
    }
}
