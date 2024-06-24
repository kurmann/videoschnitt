using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.Common.Models;
using Kurmann.Videoschnitt.Common.Services.FileSystem;
using Kurmann.Videoschnitt.Common.Services.Metadata;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

/// <summary>
/// Verantwortlich für das Lesen des Eingabeverzeichnisses mit allen Medien-Dateien und weiteren Dateien.
/// Nimmt eine Separation der Medien-Dateien vor damit diese in nachfolgenden Prozessen leichter verarbeitet werden können.
/// </summary>
public class InputDirectoryReaderService
{
    private readonly IFileSearchService _fileSearchService;
    private readonly ILogger<InputDirectoryReaderService> _logger;
    private readonly IFileOperations _fileOperations;
    private readonly FFmpegMetadataService _ffmpegMetadataService;

    public InputDirectoryReaderService(ILogger<InputDirectoryReaderService> logger,
                                       IFileSearchService fileSearchService,
                                       IFileOperations fileOperations,
                                       FFmpegMetadataService ffmpegMetadataService)
    {
        _logger = logger;
        _fileSearchService = fileSearchService;
        _fileOperations = fileOperations;
        _ffmpegMetadataService = ffmpegMetadataService;
    }

    public async Task<Result<InputDirectoryContent>> ReadInputDirectoryAsync(string inputDirectory)
    {
        var mediaSets = new List<MediaSet>();
        var masterfiles = new List<Masterfile>();
        var ignoredFiles = new List<IgnoredFile>();

        if (string.IsNullOrWhiteSpace(inputDirectory))
        {
            return Result.Failure<InputDirectoryContent>("Eingabeverzeichnis ist leer.");
        }

        // Prüfe ob das Eingabeverzeichnis existiert
        if (!Directory.Exists(inputDirectory))
        {
            return Result.Failure<InputDirectoryContent>($"Eingabeverzeichnis {inputDirectory} existiert nicht.");
        }
        var inputDirectoryInfo = new DirectoryInfo(inputDirectory);

        // Suche alle Dateien im Eingabeverzeichnis einschliesslich aller Unterverzeichnisse und separiere diese
        await foreach (var file in _fileSearchService.GetFilesAsync(inputDirectoryInfo, SearchOption.AllDirectories))
        {
            // Wenn die Datei versteckt ist, füge sie zu den ignorierten Dateien hinzu
            if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                _logger.LogInformation($"Die Datei {file.FullName} ist versteckt und wird ignoriert.");
                ignoredFiles.Add(new IgnoredFile(file, IgnoredFileReason.Hidden));
                continue;
            }

            // Unterverzeichnisse werden ignoriert, also füge alle Unterverzeichnisse zu den ignorierten Dateien hinzu
            if (file.Attributes.HasFlag(FileAttributes.Directory))
            {
                _logger.LogInformation($"Das Verzeichnis {file.FullName} wird ignoriert.");
                ignoredFiles.Add(new IgnoredFile(file, IgnoredFileReason.Directory));
                continue;
            }

            // Dateien, die sich in einem Unterverzeichnis befinden, werden ignoriert
            if (file.DirectoryName != inputDirectory)
            {
                _logger.LogInformation($"Die Datei {file.FullName} befindet sich in einem Unterverzeichnis und wird ignoriert.");
                ignoredFiles.Add(new IgnoredFile(file, IgnoredFileReason.LocatedInSubDirectory));
                continue;
            }

            // Dateien, die gerade in Verwendung sind, werden ignoriert.
            var isFileInUse = await _fileOperations.IsFileInUseAsync(file.FullName);
            if (isFileInUse.IsFailure)
            {
                _logger.LogWarning($"Fehler beim Prüfen ob die Datei {file.FullName} gerade in Verwendung ist: {isFileInUse.Error}");
                _logger.LogInformation("Die Datei wird ignoriert mit Vermerk 'IgnoredFileReason.NotDefined'.");
                ignoredFiles.Add(new IgnoredFile(file, IgnoredFileReason.NotDefined));
                continue;
            }
            else if (isFileInUse.Value)
            {
                _logger.LogInformation($"Die Datei {file.FullName} wird ignoriert, weil sie gerade in Verwendung ist.");
                ignoredFiles.Add(new IgnoredFile(file, IgnoredFileReason.FileInUse));
                continue;
            }

            // Prüfe ob die Datei eine Masterdatei ist
            var isMasterfileResult = await IsMasterfile(file);
            if (isMasterfileResult.IsFailure)
            {
                _logger.LogWarning($"Fehler beim Prüfen ob die Datei {file.FullName} eine Masterdatei ist: {isMasterfileResult.Error}");
                _logger.LogInformation("Die Datei wird ignoriert mit Vermerk 'IgnoredFileReason.NotDefined'.");
                ignoredFiles.Add(new IgnoredFile(file, IgnoredFileReason.NotDefined));
                continue;
            }
            if (isMasterfileResult.Value.HasValue)
            {
                masterfiles.Add(isMasterfileResult.Value.Value);
                continue;
            }

            // todo: Alle unterstützten Medien-Dateien in Mediensets gruppieren

            // die anderen dateine als IgnoredFileReason.NotSupported festlegen
        }


        return new InputDirectoryContent(mediaSets, masterfiles, ignoredFiles);
    }

    /// <summary>
    /// Gibt zurück, ob die Datei eine Masterdatei ist.
    /// Wenn die Datei keine Quicktime-Datei ist, ist sie keine Masterdatei.
    /// Wenn die Datei eine Quicktime-Datei ist, prüfe ob sie den Codec Apple ProRes hat.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private async Task<Result<Maybe<Masterfile>>> IsMasterfile(FileInfo file)
    {
        _logger.LogInformation($"Prüfe ob die Datei {file.FullName} eine Masterdatei ist.");
        var isQuickTimeFileExtension = QuickTimeMovie.IsQuickTimeMovieExtension(file);

        // Wenn die Datei keine Quicktime-Datei ist, ist sie keine Masterdatei
        if (!isQuickTimeFileExtension)
        {
            _logger.LogInformation($"Die Datei {file.FullName} ist keine Quicktime-Datei und wird nicht als Masterdatei betrachtet.");
            return Result.Success<Maybe<Masterfile>>(Maybe<Masterfile>.None);
        }

        // Wenn die Datei eine Quicktime-Datei ist, prüfe ob sie den Codec Apple ProRes hat
        string? codecName = null;
        var metadataResult = await _ffmpegMetadataService.GetVideoCodecNameAsync(file.FullName);
        if (metadataResult.IsFailure)
        {
            return Result.Failure<Maybe<Masterfile>> ($"Fehler beim Extrahieren des Metadaten-Feldes 'codec_name' aus der Datei {file.FullName}: {metadataResult.Error}");
        }
        codecName = metadataResult.Value;
        _logger.LogInformation($"Der Codec der Datei {file.FullName} ist {codecName}.");

        // Wenn der Codec Apple ProRes ist, ist die Datei eine Masterdatei
        if (codecName == "prores")
        {
            _logger.LogInformation($"Die Datei wird als Masterdatei betrachtet weil der Codec Apple ProRes ist und die Datei eine Quicktime-Datei ist.");

            // Rufe noch zur Vollständigkeit das Codec-Profil ab
            string? codecProfile = null;
            var codecProfileResult = await _ffmpegMetadataService.GetVideoCodecProfileAsync(file.FullName);
            if (codecProfileResult.IsFailure)
            {
                _logger.LogWarning($"Fehler beim Extrahieren des Metadaten-Feldes 'profile' aus der Datei {file.FullName}: {codecProfileResult.Error}");
                _logger.LogInformation("Das Feld 'profile' wird nicht weiter berücksichtigt.");
            }
            else
            {
                codecProfile = codecProfileResult.Value;
            }

            return Result.Success<Maybe<Masterfile>>(new Masterfile(file, codecName, codecProfile));
        }

        _logger.LogInformation($"Die Datei {file.FullName} ist keine Masterdatei, weil der Codec nicht Apple ProRes ist.");
        return Result.Success<Maybe<Masterfile>>(Maybe<Masterfile>.None);
    }
}

public record InputDirectoryContent(List<MediaSet> MediaSets, List<Masterfile> Masterfiles, List<IgnoredFile> IgnoredFiles);