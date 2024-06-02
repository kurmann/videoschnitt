using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.Features.MetadataProcessor;

public class MetadataProcessingService
{
    private readonly ILogger<MetadataProcessingService> _logger;
    private readonly DirectoryInfo _metadataProcessingDirectory;

    public MetadataProcessingService(ILogger<MetadataProcessingService> logger)
    {
        _logger = logger;

        // Lade Umgebungsvariablen
        _metadataProcessingDirectory = new DirectoryInfo(Environment.GetEnvironmentVariable("METADATA_PROCESSING_DIRECTORY"));
    }

    public Task ProcessMetadataAsync()
    {
        _logger.LogInformation("Metadatenverarbeitung gestartet.");

        if (!_metadataProcessingDirectory.Exists)
        {
            _logger.LogWarning("Verzeichnis {directory} existiert nicht.", _metadataProcessingDirectory.FullName);
            return Task.CompletedTask;
        }
        _logger.LogInformation("Verzeichnis für Metadatenverarbeitung: {directory}", _metadataProcessingDirectory.FullName);
        
        // todo: integrate message service
        // OnMetadataProcessingEvent(new MetadataProcessingEventArgs("Metadatenverarbeitung gestartet auf Verzeichnis " + _metadataProcessingDirectory.FullName));

        _logger.LogInformation("Metadatenverarbeitung abgeschlossen.");
        return Task.CompletedTask;
    }
}

public class MetadataProcessingEventArgs : EventArgs
{
    public MetadataProcessingEventArgs(string message)
    {
        Message = message;
    }

    public string Message { get; }
}