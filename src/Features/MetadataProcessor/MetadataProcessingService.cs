using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.Features.MetadataProcessor.Handler;
using Kurmann.Videoschnitt.Features.MetadataProcessor.Events;
using Wolverine;

namespace Kurmann.Videoschnitt.Features.MetadataProcessor;

public class MetadataProcessingService
{
    private readonly ILogger<MetadataProcessingService> _logger;
    private readonly DirectoryInfo? _metadataProcessingDirectory;
    private readonly IMessageBus _bus;

    public MetadataProcessingService(ILogger<MetadataProcessingService> logger, IMessageBus bus)
    {
        _logger = logger;
        _bus = bus;

        // Lade Umgebungsvariablen
        var metadataProcessingDirectoryValue = Environment.GetEnvironmentVariable("METADATA_PROCESSING_DIRECTORY");
        if (string.IsNullOrEmpty(metadataProcessingDirectoryValue))
        {
            _logger.LogWarning("Umgebungsvariable METADATA_PROCESSING_DIRECTORY ist nicht gesetzt.");
            return;
        }
        var directory = new DirectoryInfo(metadataProcessingDirectoryValue);
        if (directory == null || !directory.Exists)
        {
            _logger.LogWarning("Ungültiger oder nicht existierender Wert für Umgebungsvariable METADATA_PROCESSING_DIRECTORY: {value}", metadataProcessingDirectoryValue);
            return;
        }

        _metadataProcessingDirectory = directory;
    }

    public async Task ProcessMetadataAsync()
    {
        _logger.LogInformation("Metadatenverarbeitung gestartet.");

        if (_metadataProcessingDirectory == null || !_metadataProcessingDirectory.Exists)
        {
            _logger.LogWarning("Verzeichnis existiert nicht.");
            return;
        }
        _logger.LogInformation("Verzeichnis für Metadatenverarbeitung: {directory}", _metadataProcessingDirectory.FullName);

        // Simuliere Metadatenverarbeitung
        await Task.Delay(3000);

        // todo: Veröffentliche Nachricht über abgeschlossene Metadatenverarbeitung
    
        _logger.LogInformation("Metadatenverarbeitung abgeschlossen.");

        await _bus.PublishAsync(new MetadataProcessedEvent($"Metadatanverarbeitung für Verzeichnis {_metadataProcessingDirectory.FullName} abgeschlossen."));
        
    }
}