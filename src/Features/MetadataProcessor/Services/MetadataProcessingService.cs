using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.Features.MetadataProcessor.Services;

public class MetadataProcessingService
{
    private readonly ILogger<MetadataProcessingService> _logger;
    private readonly Settings _settings;

    public MetadataProcessingService(ILogger<MetadataProcessingService> logger, IOptions<Settings> options)
    {
        _logger = logger;
        _settings = options.Value;
    }

    public Task ProcessMetadataAsync()
    {
        _logger.LogInformation("Metadatenverarbeitung gestartet.");

        if (_settings.NewOriginalMediaDirectories != null)
        {
            foreach (var directory in _settings.NewOriginalMediaDirectories)
            {
                _logger.LogInformation("Verzeichnisse zur Verarbeitung: {directory}", directory);
                // Implementiere die eigentliche Verarbeitungslogik hier
            }
        }

        _logger.LogInformation("Metadatenverarbeitung abgeschlossen.");
        return Task.CompletedTask;
    }
}