using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Kurmann.Videoschnitt.Common.Services;
using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using System.Reflection;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder;

public class GenerateMediaSetIndexWorkflow
{
    public const string WorkflowName = "MediaSetIndex";

    private readonly ILogger<MetadataXmlWorkflow> _logger;
    private readonly ApplicationSettings _applicationSettings;

    public GenerateMediaSetIndexWorkflow(ILogger<GenerateMediaSetIndexWorkflow> logger, IOptions<ApplicationSettings> applicationSettings)
    {
        _logger = logger;
        _applicationSettings = applicationSettings.Value;
    }

    public Task<Result> ExecuteAsync()
    {
        _logger.LogInformation("Starting GenerateMediaSetIndexWorkflow...");


        return Task.FromResult(Result.Success());
    }
}
