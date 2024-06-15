using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public class Engine
{
    private readonly ModuleSettings _moduleSettings;
    private readonly ApplicationSettings _applicationSettings;
    private readonly ILogger<Engine> _logger;
    private readonly InfuseMetadataXmlService _infuseMetadataXmlService;
    private readonly TargetDirectoryResolver _targetDirectoryResolver;
    private readonly MediaIntegratorService _mediaIntegratorService;

    public Engine(IOptions<ModuleSettings> moduleSettings,
                  IOptions<ApplicationSettings> applicationSettings,
                  ILogger<Engine> logger,
                  InfuseMetadataXmlService infuseMetadataXmlService,
                  TargetDirectoryResolver targetDirectoryResolver,
                  MediaIntegratorService mediaIntegratorService)
    {
        _moduleSettings = moduleSettings.Value;
        _applicationSettings = applicationSettings.Value;
        _logger = logger;
        _infuseMetadataXmlService = infuseMetadataXmlService;
        _targetDirectoryResolver = targetDirectoryResolver;
        _mediaIntegratorService = mediaIntegratorService;
    }

    public Result Start(IProgress<string> progress)
    {
        progress.Report("InfuseMediaLibrary-Feature gestartet.");

        // Prüfe den Infuse-Mediathek-Pfad aus den Einstellungen
        if (_applicationSettings.InfuseMediaLibraryPath == null)
        {
            return Result.Failure($"Kein Infuse-Mediathek-Verzeichnis konfiguriert. Wenn Umgebungsvariablen verwendet werden, sollte der Name der Umgebungsvariable '{ApplicationSettings.SectionName}__{nameof(_applicationSettings.InfuseMediaLibraryPath)}' lauten.");
        }
        if (!Directory.Exists(_applicationSettings.InfuseMediaLibraryPath))
        {
            return Result.Failure($"Das Infuse-Mediathek-Verzeichnis {_applicationSettings.InfuseMediaLibraryPath} existiert nicht.");
        }

        // Prüfe das Quellverzeichnis aus den Einstellungen
        if (_applicationSettings.InputDirectory == null)
        {
            return Result.Failure($"Kein Quellverzeichnis konfiguriert. Wenn Umgebungsvariablen verwendet werden, sollte der Name der Umgebungsvariable '{ApplicationSettings.SectionName}__{nameof(_applicationSettings.InputDirectory)}' lauten.");
        }
        if (!Directory.Exists(_applicationSettings.InputDirectory))
        {
            return Result.Failure($"Das Quellverzeichnis {_applicationSettings.InputDirectory} existiert nicht.");
        }

        // Informiere über das Quellverzeichnis
        progress.Report($"Quellverzeichnis für die Integration von Medien in die Infuse-Mediathek: {_applicationSettings.InputDirectory}");

        // Versuche, Infuse-Metadaten-XML-Dateien zu finden
        var infuseMetadataXmlFiles = _infuseMetadataXmlService.GetInfuseMetadataXmlFiles(_applicationSettings.InputDirectory);
        if (infuseMetadataXmlFiles.IsFailure)
        {
            return Result.Failure($"Fehler beim Ermitteln der Infuse-Metadaten-XML-Dateien: {infuseMetadataXmlFiles.Error}");
        }

        // Informiere über die Anzahl der gefundenen Infuse-Metadaten-XML-Dateien
        progress.Report($"Anzahl der gefundenen Infuse-Metadaten-XML-Dateien: {infuseMetadataXmlFiles.Value.Count}");

        // Iteriere über alle gefundenen Infuse-Metadaten-XML-Dateien
        foreach (var infuseMetadataXmlFile in infuseMetadataXmlFiles.Value)
        {
            // Informiere über die gefundene Infuse-Metadaten-XML-Datei
            progress.Report($"Infuse-Metadaten-XML-Datei: {infuseMetadataXmlFile.FileInfo.FullName}");

            // Ermittle das Verzeichnis der Infuse-Metadaten-XML-Datei
            var infuseMetadataXmlFileDirectory = infuseMetadataXmlFile.FileInfo.Directory;
            if (infuseMetadataXmlFileDirectory == null)
            {
                _logger.LogWarning($"Das Verzeichnis der Infuse-Metadaten-XML-Datei {infuseMetadataXmlFile.FileInfo.FullName} konnte nicht ermittelt werden.");
                _logger.LogInformation("Die Datei wird ignoriert.");
                continue;
            }

            // Die gefundenen XML-Dateien repräsentieren ein Medienset. Suche alle Medien-Dateien im Medienset.
            // Dies sind alle Dateien im selben Verzeichnis wie die jeweilige XML-Datei und mit dem gleichen Dateinamen beginnt, jedoch ohne Dateiendung.
            // Beispiel: "Film.xml" -> "Film-4K.mp4", "Film-4K.srt", "Film-1080p.mp4", "Film-1080p.srt"
            var mediaSetFiles = infuseMetadataXmlFileDirectory.GetFiles($"{Path.GetFileNameWithoutExtension(infuseMetadataXmlFile.FileInfo.Name)}*").ToList();

            // Informiere über das Album und das Aufnahmedatum, das in den Infuse-Metadaten gefunden wurde und nun für die Benennung des Zielverzeichnisses verwendet wird
            var recordingDateIsoString = infuseMetadataXmlFile.Metadata.Published != null ? infuseMetadataXmlFile.Metadata.Published.Value.ToString("yyyy-MM-dd") : "unbekannt";
            progress.Report($"Folgendes Album wurde in den Infuse-Metadaten gefunden: {infuseMetadataXmlFile.Metadata.Album}");
            progress.Report($"Folgendes Aufnahmedatum wurde in den Infuse-Metadaten gefunden: {recordingDateIsoString}");

            // Ermittle mit dem TargetDirectoryResolver das Zielverzeichnis für die Medien-Dateien
            var targetDirectoryResult = _targetDirectoryResolver.ResolveTargetDirectory(mediaSetFiles, _applicationSettings.InfuseMediaLibraryPath);
            if (targetDirectoryResult.IsFailure)
            {
                _logger.LogWarning($"Fehler beim Ermitteln des Zielverzeichnisses für die Medien-Dateien im Medienset {infuseMetadataXmlFile.FileInfo.FullName}: {targetDirectoryResult.Error}");
                _logger.LogInformation("Das Medienset wird ignoriert.");
                continue;
            }

            // Informiere wohin die aktuelle Datei (Dateiname) verschoben werden soll
            progress.Report($"Verschiebe Medienset {infuseMetadataXmlFile.FileInfo.Name} nach {targetDirectoryResult.Value.FullName}");

            // Prüfe, ob die Suffixe für die Integration von Medien-Dateien in das Infuse-Mediathek-Verzeichnis konfiguriert sind
            var suffixesToIntegrate = _moduleSettings.VideoVersionSuffixesToIntegrate;
            if (suffixesToIntegrate == null || suffixesToIntegrate.Any() == false)
            {
                // Wenn keine Suffixe konfiguriert sind, bricht den Vorgang ab
                return Result.Failure("Keine Suffixe für die Integration von Medien-Dateien in das Infuse-Mediathek-Verzeichnis konfiguriert.");
            }

            // Versuche, die Medien-Dateien in das Zielverzeichnis zu verschieben
            var moveMediaFilesResult = _mediaIntegratorService.IntegrateMediaSet(mediaSetFiles, targetDirectoryResult.Value, suffixesToIntegrate, recordingDateIsoString);
            if (moveMediaFilesResult.IsFailure)
            {
                progress.Report($"Fehler beim Verschieben der Medien-Dateien in das Infuse-Mediathek-Verzeichnis {targetDirectoryResult.Value.FullName}: {moveMediaFilesResult.Error}");
                progress.Report("Das Medienset wird ignoriert.");
                continue;
            }

            // Entferne das Aufnahmedatum aus dem Infuse-Metadaten-XML-Dateinamen.
            // Das Aufnahmedatum ist im ISO-Fomat (yyyy-MM-dd) und wird durch einen Leerzeichen getrennt vom restlichen Dateinamen. Es steht am Anfang des Dateinamens.
            var infuseMetadataXmlFileNameWithoutRecordingDate = infuseMetadataXmlFile.FileInfo.Name.Replace($"{recordingDateIsoString} ", string.Empty);

            // Kopiere das Infuse-Metadaten-XML-Datei in das Infuse-Mediathek-Verzeichnis damit die Infuse-Mediathek die Metadaten lesen kann
            var targetInfuseMetadataXmlFilePath = Path.Combine(targetDirectoryResult.Value.FullName, infuseMetadataXmlFileNameWithoutRecordingDate);
            try
            {
                File.Copy(infuseMetadataXmlFile.FileInfo.FullName, targetInfuseMetadataXmlFilePath, true);
                progress.Report($"Infuse-Metadaten-XML-Datei {infuseMetadataXmlFile.FileInfo.FullName} wurde erfolgreich in das Infuse-Mediathek-Verzeichnis {targetDirectoryResult.Value.FullName} kopiert.");
            }
            catch (Exception ex)
            {
                progress.Report($"Fehler beim Kopieren der Infuse-Metadaten-XML-Datei {infuseMetadataXmlFile.FileInfo.FullName} in das Infuse-Mediathek-Verzeichnis {targetDirectoryResult.Value.FullName}: {ex.Message}");
                progress.Report("Das Medienset wird ignoriert.");
                continue;
            }

            progress.Report($"Medienset {infuseMetadataXmlFile.FileInfo.Name} wurde erfolgreich in das Infuse-Mediathek-Verzeichnis {targetDirectoryResult.Value.FullName} verschoben.");
        }

        progress.Report("InfuseMediaLibrary-Feature beendet.");

        return Result.Success();
    }
}