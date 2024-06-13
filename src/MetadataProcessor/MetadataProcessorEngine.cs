using Kurmann.Videoschnitt.MetadataProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;
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
        private readonly MediaSetVariantService _mediaSetVariantService;

        public MetadataProcessorEngine(IOptions<MetadataProcessorSettings> settings, ILogger<MetadataProcessorEngine> logger,
            MediaFileListenerService mediaFileListenerService, MetadataProcessingService metadataProcessingService, 
            FFmpegMetadataService ffmpegMetadataService, MediaTypeDetectorService mediaTypeDetectorService,
            MediaSetVariantService mediaSetVariantService)
        {
            _settings = settings.Value;
            _mediaFileListenerService = mediaFileListenerService;
            _metadataProcessingService = metadataProcessingService;
            _ffmpegMetadataService = ffmpegMetadataService;
            _mediaTypeDetectorService = mediaTypeDetectorService;
            _mediaSetVariantService = mediaSetVariantService;
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

                // Wenn die Datei ein Mpeg4-Video ist, ermittle die QuickTime-Movie-Variante
                if (mediaTypeResult.Value is Mpeg4Video mpeg4Video)
                {
                    var quickTimeMovieVariantResult = _mediaSetVariantService.GetQuickTimeMovieVariant(mpeg4Video, mediaFiles.Value);
                    if (quickTimeMovieVariantResult.IsFailure)
                    {
                        progress.Report($"Fehler beim Ermitteln der QuickTime-Movie-Variante für Mpeg4-Video {mpeg4Video.FileInfo.Name}: {quickTimeMovieVariantResult.Error}");
                        continue;
                    }

                    // Informiere über die gefundene QuickTime-Movie-Variante
                    progress.Report($"QuickTime-Movie-Variante für Mpeg4-Video {mpeg4Video.FileInfo.Name}: {quickTimeMovieVariantResult.Value.FileInfo.FullName}");
                }
            }

            return Result.Success();
        }
    
    }
}
