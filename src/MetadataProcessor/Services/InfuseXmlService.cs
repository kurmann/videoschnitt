using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using System.Xml.Linq;
using Kurmann.Videoschnitt.MetadataProcessor.Entities;
using Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;
using Kurmann.Videoschnitt.LocalFileSystem.UnixSystems;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services
{
    /// <summary>
    /// Verantwortlich für das Erstellen von Infuse-XML-Dateien.
    /// </summary>
    public class InfuseXmlService
    {
        private readonly ILogger<InfuseXmlService> _logger;
        private readonly FFmpegMetadataService _ffmpegMetadataService;
        private readonly FileTransferService _fileTransferService;

        public InfuseXmlService(ILogger<InfuseXmlService> logger, FFmpegMetadataService ffmpegMetadataService, FileTransferService fileTransferService)
        {
            _logger = logger;
            _ffmpegMetadataService = ffmpegMetadataService;
            _fileTransferService = fileTransferService;
        }

        public async Task<Result<XDocument>> ReadMetdataFromQuickTimeMovie(QuickTimeMovie quickTimeMovie, IProgress<string> progress)
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
            progress.Report($"Extrahierte Metadaten aus QuickTime-Movie {quickTimeMovie.FileInfo.Name}:\n{ffmpegMetadata.Value.RawString}");

            // Erstelle ein Infuse-XML-Objekt aus den Metadaten
            var infuseXml = ffmpegMetadata.Value.ToInfuseXml();

            return infuseXml;
        }

        public async Task<Result<XDocument>> ReadMetadataFromMpeg4Video(Mpeg4Video mpeg4Video, IProgress<string> progress)
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