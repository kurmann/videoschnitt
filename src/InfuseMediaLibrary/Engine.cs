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

    public Engine(IOptions<ModuleSettings> moduleSettings, IOptions<ApplicationSettings> applicationSettings, ILogger<Engine> logger, InfuseMetadataXmlService infuseMetadataXmlService, TargetDirectoryResolver targetDirectoryResolver)
    {
        _moduleSettings = moduleSettings.Value;
        _applicationSettings = applicationSettings.Value;
        _logger = logger;
        _infuseMetadataXmlService = infuseMetadataXmlService;
        _targetDirectoryResolver = targetDirectoryResolver;
    }

    public Result Start(IProgress<string> progress)
    {
        progress.Report("InfuseMediaLibrary-Feature gestartet.");

        // Prüfe den Infuse-Mediathek-Pfad aus den Einstellungen
        if (_applicationSettings.InfuseMediaLibraryPath == null)
        {
            return Result.Failure($"Kein Infuse-Mediathek-Verzeichnis konfiguriert. Wenn Umgebungsvariablen verwendet werden, sollte der Name der Umgebungsvariable '{ModuleSettings.SectionName}__{nameof(_applicationSettings.InfuseMediaLibraryPath)}' lauten.");
        }
        if (!Directory.Exists(_applicationSettings.InfuseMediaLibraryPath))
        {
            return Result.Failure($"Das Infuse-Mediathek-Verzeichnis {_applicationSettings.InfuseMediaLibraryPath} existiert nicht.");
        }

        // Prüfe das Quellverzeichnis aus den Einstellungen
        if (_applicationSettings.InputDirectory == null)
        {
            return Result.Failure($"Kein Quellverzeichnis konfiguriert. Wenn Umgebungsvariablen verwendet werden, sollte der Name der Umgebungsvariable '{ModuleSettings.SectionName}__{nameof(_applicationSettings.InputDirectory)}' lauten.");
        }
        if (!Directory.Exists(_applicationSettings.InputDirectory))
        {
            return Result.Failure($"Das Quellverzeichnis {_applicationSettings.InputDirectory} existiert nicht.");
        }

        // Versuche, Infuse-Metadaten-XML-Dateien zu finden
        var infuseMetadataXmlFiles = _infuseMetadataXmlService.GetInfuseMetadataXmlFiles(_applicationSettings.InfuseMediaLibraryPath);
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
            var recordingDate = infuseMetadataXmlFile.Metadata.Published != null ? infuseMetadataXmlFile.Metadata.Published.Value.ToString("yyyy-MM-dd") : "unbekannt";
            progress.Report($"Folgendes Album wurde in den Infuse-Metadaten gefunden: {infuseMetadataXmlFile.Metadata.Album}");
            progress.Report($"Folgendes Aufnahmedatum wurde in den Infuse-Metadaten gefunden: {recordingDate}");

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
            progress.Report("Diese Funktion ist noch nicht implementiert.");
        }

        progress.Report("InfuseMediaLibrary-Feature beendet.");

        return Result.Success();
    }
}