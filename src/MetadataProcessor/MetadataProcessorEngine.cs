using Kurmann.Videoschnitt.MetadataProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.MetadataProcessor.Entities;
using Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;
using CSharpFunctionalExtensions;
using System.Xml.Linq;

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

                    // Prüfe ob eine QuickTime-Movie-Variante gefunden wurde
                    if (quickTimeMovieVariantResult.Value.HasNoValue)
                    {
                        var readMpeg4MetadataResult = await ReadMetadataFromMpeg4Video(mpeg4Video, progress);
                        if (readMpeg4MetadataResult.IsFailure)
                        {
                            progress.Report(readMpeg4MetadataResult.Error);
                        }

                        // Informiere über die extrahierten Metadaten im Infuse-XML-Format
                        progress.Report($"Infuse-XML aus Metadaten von Mpeg4-Video {mpeg4Video.FileInfo.Name}: {readMpeg4MetadataResult.Value}");

                        continue;
                    }

                    // Informiere über die gefundene QuickTime-Movie-Variante
                    progress.Report($"QuickTime-Movie-Variante für Mpeg4-Video {mpeg4Video.FileInfo.Name}: {quickTimeMovieVariantResult.Value.Value.FileInfo.Name}");

                    // Extrahiere Metadaten aus der QuickTime-Movie-Variante
                    var readQuickTimeMetadataResult = await ReadMetdataFromQuickTimeMovie(quickTimeMovieVariantResult.Value.Value, progress);
                    if (readQuickTimeMetadataResult.IsFailure)
                    {
                        progress.Report(readQuickTimeMetadataResult.Error);
                    }

                    // Informiere über die extrahierten Metadaten im Infuse-XML-Format
                    progress.Report($"Infuse-XML aus Metadaten von QuickTime-Movie {quickTimeMovieVariantResult.Value.Value.FileInfo.Name}: {readQuickTimeMetadataResult.Value}");

                    continue;
                }
            }

            return Result.Success();
        }

        private async Task<Result<XDocument>> ReadMetdataFromQuickTimeMovie(QuickTimeMovie quickTimeMovie, IProgress<string> progress)
        {
            progress.Report($"Extrahiere Metadaten aus QuickTime-Movie {quickTimeMovie.FileInfo.Name}");
            var metadataResult = await _ffmpegMetadataService.GetFFmpegMetadataAsync(quickTimeMovie.FileInfo.FullName);
            if (metadataResult.IsFailure)
            {
                return Result.Failure<XDocument>(($"Fehler beim Extrahieren der Metadaten aus QuickTime-Movie {quickTimeMovie.FileInfo.Name}: {metadataResult.Error}"));
            }

            // Parse die FFMpeg-Metadaten
            var ffmpegMetadata = FFmpegMetadata.Create(metadataResult.Value);
            if (ffmpegMetadata.IsFailure)
            {
                return Result.Failure<XDocument>($"Fehler beim Parsen der extrahierten Metadaten aus QuickTime-Movie {quickTimeMovie.FileInfo.Name}: {ffmpegMetadata.Error}");
            }

            // Informiere über die extrahierten Metadaten
            progress.Report($"Extrahierte Metadaten aus QuickTime-Movie {quickTimeMovie.FileInfo.Name}: {ffmpegMetadata.Value}");

            // Erstelle ein Infuse-XML-Objekt aus den Metadaten
            var infuseXml = ffmpegMetadata.Value.ToInfuseXml();
            return infuseXml;
        }

        private async Task<Result<XDocument>> ReadMetadataFromMpeg4Video(Mpeg4Video mpeg4Video, IProgress<string> progress)
        {
            progress.Report($"Extrahiere Metadaten aus Mpeg4-Video {mpeg4Video.FileInfo.Name}");
            var metadataResult = await _ffmpegMetadataService.GetFFmpegMetadataAsync(mpeg4Video.FileInfo.FullName);
            if (metadataResult.IsFailure)
            {
                return Result.Failure<XDocument>($"Fehler beim Extrahieren der Metadaten aus Mpeg4-Video {mpeg4Video.FileInfo.Name}: {metadataResult.Error}");
            }

            // Parse die FFMpeg-Metadaten
            var ffmpegMetadata = FFmpegMetadata.Create(metadataResult.Value);
            if (ffmpegMetadata.IsFailure)
            {
                return Result.Failure<XDocument>($"Fehler beim Parsen der extrahierten Metadaten aus Mpeg4-Video {mpeg4Video.FileInfo.Name}: {ffmpegMetadata.Error}");
            }

            // Informiere über die extrahierten Metadaten
            progress.Report($"Extrahierte Metadaten aus Mpeg4-Video {mpeg4Video.FileInfo.Name}: {ffmpegMetadata.Value}");

            // Erstelle ein Infuse-XML-Objekt aus den Metadaten
            var infuseXml = ffmpegMetadata.Value.ToInfuseXml();
            progress.Report($"Infuse-XML aus Metadaten von Mpeg4-Video {mpeg4Video.FileInfo.Name}: {infuseXml}");

            return infuseXml;
        }
    
    }
}
