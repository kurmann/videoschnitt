using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary;

public class Engine
{
    private readonly Settings _settings;

    public Engine(IOptions<Settings> settings, ILogger<Engine> logger)
    {
        _settings = settings.Value;
    }

    public Result Start(IProgress<string> progress)
    {
        progress.Report("InfuseMediaLibrary-Feature gestartet.");

        // Prüfe ob die Einstellungen korrekt geladen wurden
        if (_settings.LibraryPath == null)
        {
            return Result.Failure($"Kein Infuse-Mediathek-Verzeichnis konfiguriert. Wenn Umgebungsvariablen verwendet werden, sollte der Name der Umgebungsvariable '{Settings.SectionName}__{nameof(_settings.LibraryPath)}' lauten.");
        }

        // Informiere über das Infuse-Mediathek-Verzeichnis, das verwendet wird
        progress.Report($"Infuse-Mediathek-Verzeichnis: {_settings.LibraryPath}");

        progress.Report("InfuseMediaLibrary-Feature beendet.");

        return Result.Success();
    }
}