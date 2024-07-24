using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.PresentationAssetsBuilder.Services;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder;

/// <summary>
/// Erstellt die Infuse-XML-Dateien für die Mediensets. Diese Metadaten-Dateien sind kompatibel zur Infuse-Mediathek.
/// Gleichzeitig dienen sie auch als kuratiertes Format für Metadaten, die über den Einsatzzweck als Infuse-Mediathek hinausgehen.
/// </summary>
public class MetadataXmlWorkflow
{
    public const string WorkflowName = "MetadataXml";

    private readonly ILogger<MetadataXmlWorkflow> _logger;
    private readonly MediaSetOrganizerSettings _settings;
    private readonly DirectoryInfuseXmlFileGenerator _infuseXmlFileGenarator;

    public MetadataXmlWorkflow(ILogger<MetadataXmlWorkflow> logger, IOptions<MediaSetOrganizerSettings> settings, DirectoryInfuseXmlFileGenerator infuseXmlFileGenarator)
    {
        _logger = logger;
        _settings = settings.Value;
        _infuseXmlFileGenarator = infuseXmlFileGenarator;
    }
    public async Task<Result> ExecuteAsync()
    {
        _logger.LogInformation("Starting MetadataXmlWorkflow...");

        var generateInfuseXmlFilesResult = await _infuseXmlFileGenarator.Generate(_settings.MediaSetPathLocal);
        if (generateInfuseXmlFilesResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Erstellen der Infuse-XML-Dateien: {generateInfuseXmlFilesResult.Error}");
        }

        _logger.LogInformation("MetadataXmlWorkflow finished.");
        return Result.Success();
    }
}
