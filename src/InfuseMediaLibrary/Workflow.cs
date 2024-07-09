using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services;
using Kurmann.Videoschnitt.Common.Models;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Kurmann.Videoschnitt.Common.Services.FileSystem;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public class Workflow
{
    public const string WorkflowName = "InfuseMediaLibrary";
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

    public async Task<Result> StartAsync()
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

            // Entferne Videos, die derzeit in Bearbeitung sind
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

            // Entferne Bilder, die derzeit in Bearbeitung sind
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
            // Damit soll sichergestellt sein, dass nicht die Bilder sofort verschoben werden wenn noch keine Videodatei vorhanden ist
            if (mediaServerFiles.Length == 0)
            {
                _logger.LogInformation("Für das Medienset {MediaSet} wurden keine Videodateien gefunden. Das Medienset wird ignoriert.", mediaSetDirectory.Name);
                continue;
            }

            // Für den Infuse-Medenserver darf pro Medienset nur eine Videodatei vorhanden sein
            if (mediaServerFiles.Length != 1)
            {
                _logger.LogWarning("Für das Medienset {MediaSet} wurden {Count} Videodateien gefunden. Es wird nur eine Videodatei unterstützt.", mediaSetDirectory.Name, mediaServerFiles.Length);
                continue;
            }
            var mediaServerFile = mediaServerFiles.First();

            // Starte die Integration mit dem entsprechenden Service
            _mediaIntegratorService.IntegrateMediaSetToLocalInfuseMediaLibrary(mediaServerFile, imageFiles);
        }


        return Result.Success();
    }

    public async Task<Result<List<LocalMediaServerFiles>>> StartAsync(List<MediaSet> mediaSets)
    {
        _logger.LogInformation("InfuseMediaLibrary-Feature gestartet.");

        if (_applicationSettings.InputDirectory == null)
        {
            return Result.Failure<List<LocalMediaServerFiles>>("Eingabeverzeichnis wurde nicht korrekt aus den Einstellungen geladen.");
        }

        _logger.LogInformation("Verzeichnis der lokalen Mediensets, das als Ausgangspunkt für die Integration in die Infuse-Mediathek dient: {Directory}", _applicationSettings.MediaSetPathLocal);

        _logger.LogInformation("Iteriere über alle Mediensets und versuche, die Medien-Dateien in die Infuse-Mediathek zu integrieren.");
        var integratedMediaServerFilesByMediaSet = new List<LocalMediaServerFiles>();
        foreach (var mediaSet in mediaSets)
        {
            var integrateMediaSetResult = await _mediaIntegratorService.IntegrateMediaSetToLocalInfuseMediaLibrary(mediaSet);
            if (integrateMediaSetResult.IsFailure)
            {
                _logger.LogWarning("Fehler beim Integrieren des Mediensets {Title} in die Infuse-Mediathek: {Error}", mediaSet.Title, integrateMediaSetResult.Error);
                _logger.LogInformation("Das Medienset wird ignoriert.");
                continue;
            }

            if (integrateMediaSetResult.Value.HasNoValue)
            {
                _logger.LogInformation("Das Medienset {Title} enthält keine Medien für die Integration in die Infuse-Mediathek.", mediaSet.Title);
                _logger.LogInformation("Das Medienset wird ignoriert.");
                continue;
            }

            _logger.LogInformation("Medienset {Title} wurde erfolgreich in die Infuse-Mediathek integriert.", mediaSet.Title);
            integratedMediaServerFilesByMediaSet.Add(integrateMediaSetResult.Value.Value);
        }

        return integratedMediaServerFilesByMediaSet;
    }
}