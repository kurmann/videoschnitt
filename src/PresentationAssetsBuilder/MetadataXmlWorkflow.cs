﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.PresentationAssetsBuilder.Services;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder;

public class MetadataXmlWorkflow
{
    public const string WorkflowName = "MetadataXml";

    private readonly ILogger<MetadataXmlWorkflow> _logger;
    private readonly ApplicationSettings _applicationSettings;
    private readonly DirectoryInfuseXmlFileGenerator _infuseXmlFileGenarator;

    public MetadataXmlWorkflow(ILogger<MetadataXmlWorkflow> logger, IOptions<ApplicationSettings> applicationSettings, DirectoryInfuseXmlFileGenerator infuseXmlFileGenarator)
    {
        _logger = logger;
        _applicationSettings = applicationSettings.Value;
        _infuseXmlFileGenarator = infuseXmlFileGenarator;
    }
    public async Task<Result> ExecuteAsync()
    {
        _logger.LogInformation("Starting MetadataXmlWorkflow...");

        var generateInfuseXmlFilesResult = await _infuseXmlFileGenarator.Generate(_applicationSettings.MediaSetPathLocal);
        if (generateInfuseXmlFilesResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Erstellen der Infuse-XML-Dateien: {generateInfuseXmlFilesResult.Error}");
        }

        _logger.LogInformation("MetadataXmlWorkflow finished.");
        return Result.Success();
    }
}