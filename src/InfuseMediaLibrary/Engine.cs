using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.InfuseMediaLibrary.Services;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public class Engine
{
    private readonly Settings _settings;
    private readonly ILogger<Engine> _logger;
    private readonly InfuseMetadataXmlService _infuseMetadataXmlService;
    private readonly TargetDirectoryResolver _targetDirectoryResolver;

    public Engine(IOptions<Settings> settings, ILogger<Engine> logger, InfuseMetadataXmlService infuseMetadataXmlService, TargetDirectoryResolver targetDirectoryResolver)
    {
        _settings = settings.Value;
        _logger = logger;
        _infuseMetadataXmlService = infuseMetadataXmlService;
        _targetDirectoryResolver = targetDirectoryResolver;
    }

    public Result Start(IProgress<string> progress)
    {
        progress.Report("InfuseMediaLibrary-Feature gestartet.");

        // Informiere über das Infuse-Mediathek-Verzeichnis, das verwendet wird
        progress.Report($"Infuse-Mediathek-Verzeichnis: {_settings.LibraryPath}");

        // Prüfe den Infuse-Mediathek-Pfad aus den Einstellungen
        if (_settings.LibraryPath == null)
        {
            return Result.Failure($"Kein Infuse-Mediathek-Verzeichnis konfiguriert. Wenn Umgebungsvariablen verwendet werden, sollte der Name der Umgebungsvariable '{Settings.SectionName}__{nameof(_settings.LibraryPath)}' lauten.");
        }
        if (!Directory.Exists(_settings.LibraryPath))
        {
            return Result.Failure($"Das Infuse-Mediathek-Verzeichnis {_settings.LibraryPath} existiert nicht.");
        }

        // Prüfe das Quellverzeichnis aus den Einstellungen
        if (_settings.SourceDirectoryPath == null)
        {
            return Result.Failure($"Kein Quellverzeichnis konfiguriert. Wenn Umgebungsvariablen verwendet werden, sollte der Name der Umgebungsvariable '{Settings.SectionName}__{nameof(_settings.SourceDirectoryPath)}' lauten.");
        }
        if (!Directory.Exists(_settings.SourceDirectoryPath))
        {
            return Result.Failure($"Das Quellverzeichnis {_settings.SourceDirectoryPath} existiert nicht.");
        }

        // Versuche, Infuse-Metadaten-XML-Dateien zu finden
        var infuseMetadataXmlFiles = _infuseMetadataXmlService.TryGetInfuseMetadataXmlFiles(_settings.SourceDirectoryPath);

        // Iteriere über alle gefundenen Infuse-Metadaten-XML-Dateien
        foreach (var infuseMetadataXmlFile in infuseMetadataXmlFiles)
        {
            // Informiere über die gefundene Infuse-Metadaten-XML-Datei
            progress.Report($"Gefundene Infuse-Metadaten-XML-Datei: {infuseMetadataXmlFile.FullName}");

            // Ermittle das Verzeichnis der Infuse-Metadaten-XML-Datei
            var infuseMetadataXmlFileDirectory = infuseMetadataXmlFile.Directory;
            if (infuseMetadataXmlFileDirectory == null)
            {
                _logger.LogWarning($"Das Verzeichnis der Infuse-Metadaten-XML-Datei {infuseMetadataXmlFile.FullName} konnte nicht ermittelt werden.");
                _logger.LogInformation("Die Datei wird ignoriert.");
                continue;
            }

            // Die gefundenen XML-Dateien repräsentieren ein Medienset. Suche alle Medien-Dateien im Medienset.
            // Dies sind alle Dateien im selben Verzeichnis wie die jeweilige XML-Datei und mit dem gleichen Dateinamen beginnt, jedoch ohne Dateiendung.
            // Beispiel: "Film.xml" -> "Film-4K.mp4", "Film-4K.srt", "Film-1080p.mp4", "Film-1080p.srt"
            var mediaSetFiles = infuseMetadataXmlFileDirectory.GetFiles($"{Path.GetFileNameWithoutExtension(infuseMetadataXmlFile.Name)}*").ToList();

            // Ermittle mit dem TargetDirectoryResolver das Zielverzeichnis für die Medien-Dateien
            var targetDirectoryResult = _targetDirectoryResolver.ResolveTargetDirectory(mediaSetFiles, _settings.LibraryPath);
            if (targetDirectoryResult.IsFailure)
            {
                _logger.LogWarning($"Fehler beim Ermitteln des Zielverzeichnisses für die Medien-Dateien im Medienset {infuseMetadataXmlFile.FullName}: {targetDirectoryResult.Error}");
                _logger.LogInformation("Das Medienset wird ignoriert.");
                continue;
            }
        }

        progress.Report("InfuseMediaLibrary-Feature beendet.");

        return Result.Success();
    }
}