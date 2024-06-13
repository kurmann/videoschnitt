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
        private readonly FFmpegMetadataService _ffmpegMetadataService;

        public MetadataProcessorEngine(IOptions<MetadataProcessorSettings> settings, ILogger<MetadataProcessorEngine> logger,
            MediaFileListenerService mediaFileListenerService, MetadataProcessingService metadataProcessingService, FFmpegMetadataService ffmpegMetadataService)
        {
            _settings = settings.Value;
            _mediaFileListenerService = mediaFileListenerService;
            _metadataProcessingService = metadataProcessingService;
            _ffmpegMetadataService = ffmpegMetadataService;
        }

        public Result Start(IProgress<string> progress)
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

            // Verarbeite alle Mediensets
            var result = Process(mediaSets, progress);
            if (result.IsFailure)
            {
                return Result.Failure($"Fehler bei der Verarbeitung der Mediensets: {result.Error}");
            }

            return Result.Success();
        }

        /// <summary>
        /// Verarbeitet die Metadaten eines Mediensets.
        /// </summary>
        private Result Process(MediasetCollection mediasetCollection, IProgress<string> progress)
        {
            if (mediasetCollection?.Mediasets == null)
            {
                return Result.Failure("Keine Mediensets gefunden.");
            }

            // Iteriere über alle Mediensets
            foreach (var mediaSet in mediasetCollection.Mediasets)
            {
                // Verarbeite das Medienset
                var result = Process(mediaSet, progress);
                if (result.IsFailure)
                {
                    return Result.Failure($"Fehler bei der Verarbeitung des Mediensets {mediaSet.Name}: {result.Error}");
                }
                progress.Report($"Medienset {mediaSet.Name} erfolgreich verarbeitet.");
            }

            return Result.Success();
        }

        private Result Process(MediaSet mediaSet, IProgress<string> progress)
        {
            // Nimm die zuerst gefundene QuickTime MOV Datei als Referenz für die Metadaten-Verarbeitung
            var referenceMediaFile = mediaSet.QuickTimeVideos.FirstOrDefault();
            if (referenceMediaFile == null)
            {
                return Result.Failure($"Keine QuickTime MOV Datei gefunden im Medienset {mediaSet.Name} um die Metadaten auszulesen.");
            }

            // Informiere über den Beginn der Metadaten-Verarbeitung
            progress.Report("Beginne mit der Verarbeitung der Metadaten.");

            // Lies die Metadaten der Referenzdatei aus
            var metadataResult = _ffmpegMetadataService.GetFFmpegMetadata(referenceMediaFile.FullName);
            if (metadataResult.IsFailure)
            {
                return Result.Failure($"Fehler beim Auslesen der Metadaten der Referenzdatei {referenceMediaFile.Name}: {metadataResult.Error}");
            }

            // Informiere über die erfolgreiche Verarbeitung der Metadaten
            progress.Report("Metadaten erfolgreich verarbeitet.");

            // Informiere über die Metadaten der Referenzdatei
            progress.Report(metadataResult.Value);

            return Result.Success();
        }
    }
}
