using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

/// <summary>
/// Verantwortlich für die Verwaltung von Medienset-Varianten.
public class MediaSetVariantService
{
    private readonly MetadataProcessorSettings _settings;
    private readonly ILogger<MediaSetVariantService> _logger;

    public MediaSetVariantService(IOptions<MetadataProcessorSettings> settings, ILogger<MediaSetVariantService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Gibt die QuickTime-Movie-Variante eines Mpeg4-Videos zurück, die in den gegebenen Medien-Dateien gefunden wurde.
    /// Die Varianten werden anhand von den Variantensuffixen ermittelt, die in den Einstellungen konfiguriert sind.
    /// </summary>
    public Result<Maybe<QuickTimeMovie>> GetQuickTimeMovieVariant(Mpeg4Video mpeg4Video, IEnumerable<FileInfo> mediaFiles)
    {
        _logger.LogInformation($"Ermittle QuickTime-Movie-Variante für Mpeg4-Video {mpeg4Video.FileInfo.FullName}");

        var variantSuffixes = _settings.MediaSetSettings?.VideoVersionSuffixes;
        if (variantSuffixes == null || variantSuffixes.Count == 0)
        {
            return Result.Failure<Maybe<QuickTimeMovie>>("Keine Variantensuffixe für QuickTime-Movie-Varianten konfiguriert.");
        }

        // Suche für das gegebene Mpeg4-Video nach einer passenden QuickTime-Movie-Variante in den gegebenen Medien-Dateien
        // indem die erste Datei zurückgegeben wird mit identischem Dateinamen, aber mit einem der Variantensuffixe und ohne Dateiendung

        // Nimm als Ausgangslage den Dateinamen des Mpeg4-Videos ohne Variantensuffix und ohne Dateiendung
        var baseFileName = Path.GetFileNameWithoutExtension(mpeg4Video.FileInfo.Name);
        foreach (var variantSuffix in variantSuffixes)
        {
            // Entferne das Variantensuffix, falls vorhanden
            var baseFileNameWithoutSuffix = baseFileName.Replace(variantSuffix, string.Empty, StringComparison.InvariantCultureIgnoreCase);

            // Suche nach einer passenden QuickTime-Movie-Variante in den gegebenen Medien-Dateien, die mit dem bereinigten Dateinamen beginnt
            var quickTimeMovie = mediaFiles
                .Select(QuickTimeMovie.Create)
                .Where(result => result.IsSuccess)
                .Select(result => result.Value)
                .FirstOrDefault(quickTimeMovie => Path.GetFileNameWithoutExtension(quickTimeMovie.FileInfo.Name).StartsWith(baseFileNameWithoutSuffix, StringComparison.InvariantCultureIgnoreCase));

            // Wenn eine passende QuickTime-Movie-Variante gefunden wurde, gib sie zurück und beende die Suche
            if (quickTimeMovie != null)
            {
                _logger.LogInformation($"QuickTime-Movie-Variante für Mpeg4-Video {mpeg4Video.FileInfo.FullName} gefunden: {quickTimeMovie.FileInfo.FullName}");
                return Result.Success<Maybe<QuickTimeMovie>>(quickTimeMovie);
            }
        }

        // Wenn keine passende QuickTime-Movie-Variante gefunden wurde, gib None zurück. Dies ist kein Fehler.
        return Result.Success<Maybe<QuickTimeMovie>>(Maybe<QuickTimeMovie>.None);
    }
}