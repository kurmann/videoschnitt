using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class Engine(IOptions<Settings> settings,
                  ILogger<Engine> logger,
                  IMessageBus bus,
                  MediaFileListenerService mediaFileListenerService,
                  MetadataProcessingService metadataProcessingService)
    {
        private readonly Settings _settings = settings.Value;
        private readonly ILogger<Engine> _logger = logger;
        private readonly IMessageBus _bus = bus;
        private readonly MediaFileListenerService _mediaFileListenerService = mediaFileListenerService;
        private readonly MetadataProcessingService _metadataProcessingService = metadataProcessingService;

        public async Task StartAsync()
        {
            // Liste alle unterstützten Medien-Dateien im Verzeichnis auf
            var mediaFiles = _mediaFileListenerService.GetSupportedMediaFiles();
            if (mediaFiles.IsFailure)
            {
                _logger.LogError("Fehler beim Auflisten der Medien-Dateien: {Error}", mediaFiles.Error);
                return;
            }

            // Informiere über die Anzahl der gefundenen Medien-Dateien
            await _bus.PublishAsync(new SupportedMediaFilesForMetadataProcessingFoundEvent(mediaFiles.Value));

            // todo: Verarbeite die Metadaten
        }
    }
}