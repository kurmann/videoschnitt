using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.Messages.Metadata;
using Wolverine;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.ApplicationConfiguration;

namespace Kurmann.Videoschnitt.MetadataProcessor;

public class MetadataProcessingService
{
    private readonly ILogger<MetadataProcessingService> _logger;
    private readonly DirectoryInfo? _inputDirectory;
    private readonly IMessageBus _bus;

    public MetadataProcessingService(ILogger<MetadataProcessingService> logger, IOptions<MetadataProcessingSettings> settings, IMessageBus bus)
    {
        _logger = logger;
        _bus = bus;

        // Prüfe, ob das Verzeichnis für die Metadatenverarbeitung definiert ist
        if (string.IsNullOrWhiteSpace(settings.Value?.InputDirectory))
        {
            _logger.LogWarning("Kein Wert für die Umgebungsvariable 'MetadataProcessing__InputDirectory' gesetzt.");
            return;
        }

        // Prüfe, ob das Verzeichnis für die Metadatenverarbeitung existiert
        var directory = new DirectoryInfo(settings.Value.InputDirectory);
        if (directory == null || !directory.Exists)
        {
            _logger.LogWarning("Ungültiger oder nicht existierender Wert für Umgebungsvariable METADATA_PROCESSING_DIRECTORY: {value}", settings.Value.InputDirectory);
            return;
        }

        _inputDirectory = directory;
    }

    public async Task ProcessMetadataAsync()
    {
        _logger.LogInformation("Metadatenverarbeitung gestartet.");

        if (_inputDirectory == null || !_inputDirectory.Exists)
        {
            _logger.LogWarning("Verzeichnis existiert nicht.");
            return;
        }
        _logger.LogInformation("Verzeichnis für Metadatenverarbeitung: {directory}", _inputDirectory.FullName);

        // Simuliere Metadatenverarbeitung
        await Task.Delay(3000);

        // todo: Veröffentliche Nachricht über abgeschlossene Metadatenverarbeitung
    
        _logger.LogInformation("Metadatenverarbeitung abgeschlossen.");

        await _bus.PublishAsync(new MetadataProcessedEvent(_inputDirectory));
        
    }
}