using Kurmann.Videoschnitt.MetadataProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wolverine;
using Kurmann.Videoschnitt.Messages.MediaFiles;

namespace Kurmann.Videoschnitt.MetadataProcessor
{
    /// <summary>
    /// Zentrale Steuereinheit für die Metadaten-Verarbeitung.
    /// </summary>
    public class MetadataProcessorEngine(IOptions<MetadataProcessorSettings> settings,
                                         ILogger<MetadataProcessorEngine> logger,
                                         IMessageBus bus,
                                         MediaFileListenerService mediaFileListenerService,
                                         MetadataProcessingService metadataProcessingService)
    {
        private readonly MetadataProcessorSettings _settings = settings.Value;
        private readonly ILogger<MetadataProcessorEngine> _logger = logger;
        private readonly IMessageBus _bus = bus;
        private readonly MediaFileListenerService _mediaFileListenerService = mediaFileListenerService;
        private readonly MetadataProcessingService _metadataProcessingService = metadataProcessingService;

        public async Task StartAsync()
        {
            _logger.LogInformation("Steuereinheit für die Metadaten-Verarbeitung gestartet.");

            // Liste alle unterstützten Medien-Dateien im Verzeichnis auf
            var mediaFiles = _mediaFileListenerService.GetSupportedMediaFiles();
            if (mediaFiles.IsFailure)
            {
                await _bus.PublishAsync(new MediaFilesForMetadataProcessingFoundErrorEvent(mediaFiles.Error));
                return;
            }

            // Informiere über die Anzahl der gefundenen Medien-Dateien
            await _bus.PublishAsync(new MediaFilesForMetadataProcessingFoundEvent(mediaFiles.Value));

            // Informiere über den Beginn der Metadaten-Verarbeitung
            await _bus.PublishAsync(new MediaFilesMetadataProcessingStartedEvent());

            // Verarbeite die Metadaten der Medien-Dateien
            var processedMediaFiles = await _metadataProcessingService.ProcessMetadataAsync(mediaFiles.Value);
            if (processedMediaFiles.IsFailure)
            {
                await _bus.PublishAsync(new MediaFilesMetadataProcessingErrorEvent(processedMediaFiles.Error));
                return;
            }

            // Informiere über die erfolgreiche Verarbeitung der Metadaten
            await _bus.PublishAsync(new MediaFilesMetadataProcessingSuccessEvent(processedMediaFiles.Value));

            _logger.LogInformation("Steuerung für die Metadaten-Verarbeitung abgeschlossen.");
        }
    }
}