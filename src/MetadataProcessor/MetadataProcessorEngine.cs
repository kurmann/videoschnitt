using Kurmann.Videoschnitt.MetadataProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly InfuseXmlService _infuseXmlService;

        public MetadataProcessorEngine(IOptions<MetadataProcessorSettings> settings, ILogger<MetadataProcessorEngine> logger,
            MediaFileListenerService mediaFileListenerService, MetadataProcessingService metadataProcessingService, 
            FFmpegMetadataService ffmpegMetadataService, MediaTypeDetectorService mediaTypeDetectorService,
            MediaSetVariantService mediaSetVariantService, InfuseXmlService infuseXmlService)
        {
            _settings = settings.Value;
            _mediaFileListenerService = mediaFileListenerService;
            _metadataProcessingService = metadataProcessingService;
            _ffmpegMetadataService = ffmpegMetadataService;
            _mediaTypeDetectorService = mediaTypeDetectorService;
            _mediaSetVariantService = mediaSetVariantService;
            _infuseXmlService = infuseXmlService;
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

                // Wenn die Datei ein QuickTime-Movie ist, extrahiere die Metadaten und erstelle ein Infuse-XML-Objekt
                if (mediaTypeResult.Value is QuickTimeMovie quickTimeMovie)
                {
                    var readQuickTimeMetadataResult = await _infuseXmlService.ReadMetdataFromQuickTimeMovie(quickTimeMovie, progress);
                    if (readQuickTimeMetadataResult.IsFailure)
                    {
                        progress.Report(readQuickTimeMetadataResult.Error);
                    }

                    // Informiere über die extrahierten Metadaten im Infuse-XML-Format
                    progress.Report($"Infuse-XML aus Metadaten von QuickTime-Movie {quickTimeMovie.FileInfo.Name}: {readQuickTimeMetadataResult.Value}");

                    // Speichere das Infuse-XML-Objekt
                    var infuseXml = readQuickTimeMetadataResult.Value;

                    // Erstelle den Dateinamen für das Infuse-XML-Objekt
                    var infuseXmlFileName = _mediaSetVariantService.GetInfuseXmlFileName(quickTimeMovie.FileInfo);
                    if (infuseXmlFileName.IsFailure)
                    {
                        progress.Report(infuseXmlFileName.Error);
                        
                        // Fahre mit der nächsten Datei fort
                        continue;
                    }

                    // Informiere über den Dateinamen des Infuse-XML-Objekts
                    progress.Report($"Dateiname des Infuse-XML-Objekts für Medien-Objekt {mediaFile.FullName}: {infuseXmlFileName.Value.FullName}");

                    // Schreibe die Datei
                    infuseXml.Save(infuseXmlFileName.Value.FullName);

                    // Informiere über den Erfolg
                    progress.Report($"Infuse-XML-Datei für Medien-Objekt {mediaFile.FullName} erfolgreich geschrieben.");

                    // Fahre mit der nächsten Datei fort
                    continue;
                }


                // Wenn die Datei ein Mpeg4-Video ist, dann schreibe die Infuse-XML-Datei nur wenn kein Infuse-XML-Objekt bereits besteht, 
                // da die QuickTime-Movie-Variante bevorzugt wird aufgrund der besseren Metadaten
                if (mediaTypeResult.Value is Mpeg4Video mpeg4Video)
                {
                    // Informiere über das Mpeg4-Video und was wir nun vorhaben
                    progress.Report("Mpeg4-Video gefunden. Es wird geprüft, ob bereits ein Infuse-XML-Objekt existiert.");

                    // Ermittle den Dateinamen des Infuse-XML-Objekts, nachdem wir suchen, ob bereits ein Infuse-XML-Objekt existiert
                    var infuseXmlFileName = _mediaSetVariantService.GetInfuseXmlFileName(mpeg4Video.FileInfo);
                    if (infuseXmlFileName.IsFailure)
                    {
                        progress.Report(infuseXmlFileName.Error);

                        // Fahre mit der nächsten Datei fort
                        continue;
                    }

                    // Informiere über den Dateinamen des Infuse-XML-Objekts
                    progress.Report($"Dateiname des Infuse-XML-Objekts für Medien-Objekt {mediaFile.FullName}: {infuseXmlFileName.Value.FullName}");

                    // Prüfe ob bereits ein Infuse-XML-Objekt existiert
                    if (infuseXmlFileName.Value.Exists)
                    {
                        progress.Report($"Infuse-XML-Datei für Medien-Objekt {mediaFile.FullName} existiert bereits. Es wird keine neue Datei geschrieben.");

                        // Fahre mit der nächsten Datei fort
                        continue;
                    }

                    // Informiere über das Fehlen des Infuse-XML-Objekts und was wir nun vorhaben
                    progress.Report($"Infuse-XML-Datei für Medien-Objekt {mediaFile.FullName} existiert nicht. Es wird eine neue Datei geschrieben.");

                    // Extrahiere die Metadaten und erstelle ein Infuse-XML-Objekt
                    progress.Report($"Extrahiere Metadaten aus Mpeg4-Video {mpeg4Video.FileInfo.Name}");
                    var readMpeg4MetadataResult = await _infuseXmlService.ReadMetadataFromMpeg4Video(mpeg4Video, progress);
                    if (readMpeg4MetadataResult.IsFailure)
                    {
                        progress.Report(readMpeg4MetadataResult.Error);
                        continue;
                    }

                    // Informiere über die extrahierten Metadaten im Infuse-XML-Format
                    progress.Report($"Infuse-XML aus Metadaten von Mpeg4-Video {mpeg4Video.FileInfo.Name}: {readMpeg4MetadataResult.Value}");

                    // Speichere das Infuse-XML-Objekt
                    var infuseXml = readMpeg4MetadataResult.Value;

                    // Schreibe die Datei
                    infuseXml.Save(infuseXmlFileName.Value.FullName);

                    // Informiere über den Erfolg
                    progress.Report($"Infuse-XML-Datei für Medien-Objekt {mediaFile.FullName} erfolgreich geschrieben.");
                }
            }

            return Result.Success();
        }


    
    }
}
