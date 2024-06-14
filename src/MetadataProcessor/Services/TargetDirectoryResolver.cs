using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.MetadataProcessor.Entities;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

public class TargetDirectoryResolver
{
    private readonly ILogger<TargetDirectoryResolver> _logger;
    private readonly MetadataProcessorSettings _settings;

    public TargetDirectoryResolver(IOptions<MetadataProcessorSettings> settings, ILogger<TargetDirectoryResolver> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Result<FileInfo> ResolveTargetDirectory(FFmpegMetadata ffmpegMetadata)
    {
        if (ffmpegMetadata == null)
        {
            return Result.Failure<FileInfo>("FFmpegMetadata darf nicht null sein.");
        }

        if (string.IsNullOrWhiteSpace(ffmpegMetadata.Album) || ffmpegMetadata.Date == null)
        {
            return Result.Failure<FileInfo>("FFmpegMetadata muss ein Album und ein Datum enthalten.");
        }

        var albumPath = Path.Combine(_settings.InfuseMediaLibrarySettings?.InfuseMediaLibraryPath ?? string.Empty, ffmpegMetadata.Album);
        var datePath = Path.Combine(albumPath, ffmpegMetadata.Date.Value.ToString("yyyy"), ffmpegMetadata.Date.Value.ToString("yyyy-MM-dd"));

        try
        {
            if (!Directory.Exists(datePath))
            {
                Directory.CreateDirectory(datePath);
                _logger.LogInformation($"Verzeichnis {datePath} wurde erstellt.");
            }

            return Result.Success(new FileInfo(datePath));
        }
        catch (Exception ex)
        {
            return Result.Failure<FileInfo>($"Fehler beim Erstellen des Verzeichnisses: {ex.Message}");
        }
    }
}