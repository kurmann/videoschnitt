using Kurmann.Videoschnitt.MetadataProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wolverine;
using Kurmann.Videoschnitt.Messages.MediaFiles;

namespace Kurmann.Videoschnitt.MetadataProcessor
{
    /// <summary>
    /// Zentrale Steuerungsklasse für die Metadaten-Verarbeitung.
    /// </summary>
    public class MetadataProcessorEngine(IOptions<Settings> settings,
                                         ILogger<MetadataProcessorEngine> logger,
                                         IMessageBus bus,
                                         MediaFileListenerService mediaFileListenerService,
                                         MetadataProcessingService metadataProcessingService)
    {
        private readonly Settings _settings = settings.Value;
        private readonly ILogger<MetadataProcessorEngine> _logger = logger;
        private readonly IMessageBus _bus = bus;
        private readonly MediaFileListenerService _mediaFileListenerService = mediaFileListenerService;
        private readonly MetadataProcessingService _metadataProcessingService = metadataProcessingService;

        public async Task StartAsync()
        {
            // Liste alle unterstützten Medien-Dateien im Verzeichnis auf
            var mediaFiles = _mediaFileListenerService.GetSupportedMediaFiles();
            if (mediaFiles.IsFailure)
            {
                await _bus.PublishAsync(new MediaFilesForMetadataProcessingFoundErrorEvent(mediaFiles.Error));
                return;
            }

            // Informiere über die Anzahl der gefundenen Medien-Dateien
            await _bus.PublishAsync(new MediaFilesForMetadataProcessingFoundEvent(mediaFiles.Value));

            // todo: Verarbeite die Metadaten
        }
    }
}