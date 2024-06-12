using Kurmann.Videoschnitt.MetadataProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wolverine;
using Kurmann.Videoschnitt.Messages.MediaFiles;
using Kurmann.Videoschnitt.MetadataProcessor.Entities;

namespace Kurmann.Videoschnitt.MetadataProcessor
{
    /// <summary>
    /// Zentrale Steuereinheit für die Metadaten-Verarbeitung.
    /// </summary>
    public class MetadataProcessorEngine
    {
        private readonly MetadataProcessorSettings _settings;
        private readonly ILogger<MetadataProcessorEngine> _logger;
        private readonly IMessageBus _bus;
        private readonly MediaFileListenerService _mediaFileListenerService;
        private readonly MetadataProcessingService _metadataProcessingService;

        public MetadataProcessorEngine(IOptions<MetadataProcessorSettings> settings,
                                         ILogger<MetadataProcessorEngine> logger,
                                         IMessageBus bus,
                                         MediaFileListenerService mediaFileListenerService,
                                         MetadataProcessingService metadataProcessingService)
        {
            _settings = settings.Value;
            _logger = logger;
            _bus = bus;
            _mediaFileListenerService = mediaFileListenerService;
            _metadataProcessingService = metadataProcessingService;
        }

        public async Task StartAsync(Action<WorkflowStatusUpdate>? statusCallback = null)
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

            // Gruppiere die gefundenen Medien-Dateien nach Medienset. Ein Medienset ist eine Gruppe von Medien-Dateien, die zusammengehören.
            var mediaSets = MediasetCollection.Create(mediaFiles.Value,
                                                      _settings.MediaSetSettings.VideoVersionSuffixes,
                                                      _settings.MediaSetSettings.ImageVersionSuffixes,
                                                      _settings.FileTypeSettings.SupportedVideoExtensions,
                                                      _settings.FileTypeSettings.SupportedImageExtensions);

            // Informiere über den Beginn der Metadaten-Verarbeitung
            await _bus.PublishAsync(new MediaFilesMetadataProcessingStartedEvent());

            // Verarbeite die Metadaten der Medien-Dateien.
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

public record WorkflowStatusUpdate(string Message, int Progress, bool IsError = false);