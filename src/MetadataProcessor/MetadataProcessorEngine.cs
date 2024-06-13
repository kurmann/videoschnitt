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
        private readonly MediaTypeDetectorService _mediaTypeDetectorService;

        public MetadataProcessorEngine(IOptions<MetadataProcessorSettings> settings, ILogger<MetadataProcessorEngine> logger,
            MediaFileListenerService mediaFileListenerService, MetadataProcessingService metadataProcessingService, 
            FFmpegMetadataService ffmpegMetadataService, MediaTypeDetectorService mediaTypeDetectorService)
        {
            _settings = settings.Value;
            _mediaFileListenerService = mediaFileListenerService;
            _metadataProcessingService = metadataProcessingService;
            _ffmpegMetadataService = ffmpegMetadataService;
            _mediaTypeDetectorService = mediaTypeDetectorService;
        }

        public async Task<Result> Start(IProgress<string> progress)
        {
            progress.Report("Steuereinheit für die Metadaten-Verarbeitung gestartet.");

            // Prüfe ob die Einstellungen korrekt geladen wurden
            if (_settings.InputDirectory == null)
            {
                return Result.Failure("Kein Eingabeverzeichnis konfiguriert. Wenn Umgebungsvariablen verwendet werden, sollte der Name der Umgebungsvariable 'MetadataProcessing__InputDirectory' lauten.");
            }

            // Informiere über das Eingabeverzeichnis
            progress.Report($"Eingabeverzeichnis: {_settings.InputDirectory}");

            // Liste alle unterstützten Medien-Dateien im Verzeichnis auf
            var mediaFiles = _mediaFileListenerService.GetSupportedMediaFiles();
            if (mediaFiles.IsFailure)
            {
                return Result.Failure($"Fehler beim Ermitteln der Medien-Dateien: {mediaFiles.Error}");
            }

            // Informiere über die Anzahl der gefundenen Medien-Dateien
            progress.Report($"Anzahl der gefundenen Medien-Dateien: {mediaFiles.Value.Count}");

            // Iteriere über alle Medien-Dateien und ermittle den Medientyp
            foreach (var mediaFile in mediaFiles.Value)
            {
                var mediaTypeResult = _mediaTypeDetectorService.DetectMediaType(mediaFile);
                if (mediaTypeResult.IsFailure)
                {
                    progress.Report($"Fehler beim Ermitteln des Medientyps für Datei {mediaFile.Name}: {mediaTypeResult.Error}");
                    continue;
                }

                // Informiere über den ermittelten Medientyp
                progress.Report($"Medientyp für Datei {mediaFile.Name}: {mediaTypeResult.Value.GetType().Name}");


            }

            return Result.Success();
        }
    
    }
}
