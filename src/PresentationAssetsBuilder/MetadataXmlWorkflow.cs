using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder;

public class MetadataXmlWorkflow
{
    public const string WorkflowName = "MetadataXml";

    private readonly ILogger<MetadataXmlWorkflow> _logger;
    private readonly ApplicationSettings _applicationSettings;

    public MetadataXmlWorkflow(ILogger<MetadataXmlWorkflow> logger, IOptions<ApplicationSettings> applicationSettings)
    {
        _logger = logger;
        _applicationSettings = applicationSettings.Value;
    }

    public Task<Result> ExecuteAsync()
    {
        _logger.LogInformation("Starting MetadataXmlWorkflow...");


        return Task.FromResult(Result.Success());
    }
}
