using Kurmann.Videoschnitt.MetadataProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.MetadataProcessor.Entities;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor
{
    /// <summary>
    /// Zentrale Steuereinheit für die Metadaten-Verarbeitung.
    /// </summary>
    public class MetadataProcessorEngine
    {
        private readonly MetadataProcessorSettings _settings;
        private readonly MediaFileListenerService _mediaFileListenerService;
        private readonly MetadataProcessingService _metadataProcessingService;

        public MetadataProcessorEngine(IOptions<MetadataProcessorSettings> settings, ILogger<MetadataProcessorEngine> logger,
            MediaFileListenerService mediaFileListenerService, MetadataProcessingService metadataProcessingService)
        {
            _settings = settings.Value;
            _mediaFileListenerService = mediaFileListenerService;
            _metadataProcessingService = metadataProcessingService;
        }

        public async Task<Result> StartAsync(IProgress<string> progress)
        {
            progress.Report("Steuereinheit für die Metadaten-Verarbeitung gestartet.");

            // Liste alle unterstützten Medien-Dateien im Verzeichnis auf
            var mediaFiles = _mediaFileListenerService.GetSupportedMediaFiles();
            if (mediaFiles.IsFailure)
            {
                return Result.Failure($"Fehler beim Ermitteln der Medien-Dateien: {mediaFiles.Error}");
            }

            // Informiere über die Anzahl der gefundenen Medien-Dateien
            progress.Report($"Anzahl der gefundenen Medien-Dateien: {mediaFiles.Value.Count}");

            // Liste alle Medien-Dateien auf
            foreach (var mediaFile in mediaFiles.Value)
            {
                progress.Report(mediaFile.Name);
            }

            // Gruppiere die gefundenen Medien-Dateien nach Medienset. Ein Medienset ist eine Gruppe von Medien-Dateien, die zusammengehören.
            var mediaSets = MediasetCollection.Create(mediaFiles.Value,
                                                      _settings.MediaSetSettings.VideoVersionSuffixes,
                                                      _settings.MediaSetSettings.ImageVersionSuffixes,
                                                      _settings.FileTypeSettings.SupportedVideoExtensions,
                                                      _settings.FileTypeSettings.SupportedImageExtensions);

            // Informiere über den Beginn der Metadaten-Verarbeitung
            progress.Report("Beginne mit der Verarbeitung der Metadaten.");

            // Verarbeite die Metadaten der Medien-Dateien.
            var processedMediaFiles = await _metadataProcessingService.ProcessMetadataAsync(mediaFiles.Value);
            if (processedMediaFiles.IsFailure)
            {
                progress.Report("Fehler bei der Verarbeitung der Metadaten.");
                return Result.Failure(processedMediaFiles.Error);
            }

            // Informiere über die erfolgreiche Verarbeitung der Metadaten
            progress.Report("Metadaten erfolgreich verarbeitet.");
            return Result.Success();
        }
    }
}
