using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Kurmann.Videoschnitt.Common.Services.FileSystem;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public interface IWorkflow
{
    const string WorkflowName = "InfuseMediaLibrary";

    Task<Result> ExecuteAsync();
}

internal class Workflow : IWorkflow
{
    private readonly ApplicationSettings _applicationSettings;
    private readonly ILogger<Workflow> _logger;
    private readonly MediaIntegratorService _mediaIntegratorService;
    private readonly MediaSetOrganizerSettings _mediaSetOrganizerSettings;
    private readonly IFileOperations _fileOperations;

    public Workflow(IOptions<ApplicationSettings> applicationSettings, IOptions<MediaSetOrganizerSettings> mediaSetOrganizerSettings,
        ILogger<Workflow> logger, MediaIntegratorService mediaIntegratorService, IFileOperations fileOperations)
    {
        _applicationSettings = applicationSettings.Value;
        _mediaSetOrganizerSettings = mediaSetOrganizerSettings.Value;
        _logger = logger;
        _mediaIntegratorService = mediaIntegratorService;
        _fileOperations = fileOperations;
    }

    public async Task<Result> ExecuteAsync()
    {
        var sourceDirectoryPath = _applicationSettings.MediaSetPathLocal;
        var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);

        // Prüfe ob das Verzeichnis exisitiert
        if (sourceDirectory.Exists == false)
        {
            return Result.Failure($"Das Verzeichnis {sourceDirectory} existiert nicht.");
        }

        // Alle Unterverzeichnisse des Quellverzeichnisses werden als eigenständige Mediensets betrachtet
        var mediaSetDirectories = sourceDirectory.GetDirectories();
        if (mediaSetDirectories.Length == 0)
        {
            _logger.LogInformation("Keine Mediensets im Verzeichnis {Directory} gefunden.", sourceDirectoryPath);
            return Result.Success();
        }
        var mediaSetNamesCommaseparated = string.Join(", ", mediaSetDirectories.Select(d => d.Name));
        _logger.LogInformation("Folgende Mediensets wurden im Verzeichnis {Directory} gefunden: {MediaSets}", sourceDirectoryPath, mediaSetNamesCommaseparated);

        // Ziel ist es die Dateien zusammenzusammeln pro Medienset, die relevant für die Integration sind
        foreach (var mediaSetDirectory in mediaSetDirectories)
        {
            
            // Suche nach dem Unterverzeichnis, das die Dateien für den Medienserver enthält
            var mediaServerFileDirectory = mediaSetDirectory.GetDirectories().FirstOrDefault(d => d.Name == _mediaSetOrganizerSettings.MediaSet.MediaServerFilesSubDirectoryName);

            // Suche nach dem Unterverzeichnis, das die Bilder enthält
            var imagesDirectory = mediaSetDirectory.GetDirectories().FirstOrDefault(d => d.Name == _mediaSetOrganizerSettings.MediaSet.ImageFilesSubDirectoryName);

            // Mache mit dem nächsten Medienset weiter, wenn kein Verzeichnis für Internet-Dateien und/oder Bilder gefunden wurde
            if (mediaServerFileDirectory == null || imagesDirectory == null)
            {
                _logger.LogInformation("Für das Medienset {MediaSet} wurden keine Internet-Dateien und/oder Bilder gefunden.", mediaSetDirectory.Name);
                continue;
            }

            // Nimm an, dass alle Videos fürs Internet und alle Bilder im entsprechenden Verzeichnis liegen (durch vorangehende Prozesse)
            var mediaServerFiles = mediaServerFileDirectory.GetFiles();

            // Für die Bilder sollen nur JPG-Dateien berücksichtigt werden (Dateiendung JPG und JPEG)
            var imageFiles = imagesDirectory.GetFiles().Where(f => f.Extension == ".jpg" || f.Extension == ".jpeg").ToArray();

            // Ignoriere Videos, die derzeit in Bearbeitung sind
            var mediaServerFilesNotInUse = new List<FileInfo>();
            foreach (var file in mediaServerFiles)
            {
                var isUsedResult = await _fileOperations.IsFileInUseAsync(file.FullName);
                if (isUsedResult.IsFailure)
                {
                    _logger.LogWarning("Fehler beim Prüfen, ob die Datei {File} verwendet wird: {Error}", file.Name, isUsedResult.Error);
                    continue;
                }
                if (isUsedResult.Value)
                {
                    _logger.LogInformation("Die Datei {File} wird derzeit verwendet und wird daher nicht in die Infuse-Mediathek integriert.", file.Name);
                }
                else
                {
                    mediaServerFilesNotInUse.Add(file);
                }
            }
            mediaServerFiles = mediaServerFilesNotInUse.ToArray();

            // Ignoriere Bilder, die derzeit in Bearbeitung sind
            var imageFilesNotInUse = new List<FileInfo>();
            foreach (var file in imageFiles)
            {
                var isUsedResult = await _fileOperations.IsFileInUseAsync(file.FullName);
                if (isUsedResult.IsFailure)
                {
                    _logger.LogWarning("Fehler beim Prüfen, ob die Datei {File} verwendet wird: {Error}", file.Name, isUsedResult.Error);
                    continue;
                }
                if (isUsedResult.Value)
                {
                    _logger.LogInformation("Die Datei {File} wird derzeit verwendet und wird daher nicht in die Infuse-Mediathek integriert.", file.Name);
                }
                else
                {
                    imageFilesNotInUse.Add(file);
                }
            }
            imageFiles = imageFilesNotInUse.ToArray();

            // Wenn nur Bilddateien vorhanden sind, wird das Medienset ignoriert
            // Damit soll sichergestellt sein, dass nicht die Bilder umgehend verschoben werden wenn noch keine Videodatei vorhanden ist.
            // Damit lassen sich die Bilder nach dem Export aus Final Cut Pro noch bearbeiten bspw. Zuschneiden solange der Videoexport noch nicht abgeschlossen ist.
            if (mediaServerFiles.Length == 0)
            {
                _logger.LogInformation("Für das Medienset {MediaSet} wurden keine Videodateien gefunden. Das Medienset wird ignoriert.", mediaSetDirectory.Name);
                continue;
            }

            // Für den Infuse-Medenserver darf pro Medienset nur eine Videodatei vorhanden sein. Gib eine Warnung aus, wenn mehrere Videodateien gefunden wurden.
            if (mediaServerFiles.Length != 1)
            {
                _logger.LogWarning("Für das Medienset {MediaSet} wurden {Count} Videodateien gefunden. Es wird nur eine Videodatei unterstützt.", mediaSetDirectory.Name, mediaServerFiles.Length);
            }
            var mediaServerFile = mediaServerFiles.First();

            // Starte die Integration mit dem entsprechenden Service
            var imageFilesCommaseparated = string.Join(", ", imageFiles.Select(f => f.Name));
            _logger.LogInformation("Folgende Videodatei wird in die Infuse-Mediathek integriert: {File}", mediaServerFile.Name);
            _logger.LogInformation("Folgende Bilder werden in die Infuse-Mediathek integriert: {Files}", imageFilesCommaseparated);

            await _mediaIntegratorService.IntegrateToLocalInfuseMediaLibrary(mediaServerFile, imageFiles);
        }

        return Result.Success();
    }
}