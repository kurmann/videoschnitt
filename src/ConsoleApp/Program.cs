using System.Diagnostics;
using System.Globalization;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.ConfigurationModule;
using Kurmann.Videoschnitt.ConfigurationModule.Services;
using Kurmann.Videoschnitt.HealthCheck;
using Kurmann.Videoschnitt.Common;
using Kurmann.Videoschnitt.MediaSetOrganizer;
using Kurmann.Videoschnitt.InfuseMediaLibrary;
using Kurmann.Videoschnitt.PresentationAssetsBuilder;

namespace Kurmann.Videoschnitt.ConsoleApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        var stopwatch = Stopwatch.StartNew();

        var host = CreateHostBuilder(args).Build();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        var configurationInfoService = host.Services.GetRequiredService<ConfigurationInfoService>();
        configurationInfoService.LogConfigurationInfo();

        var exitCode = Parser.Default.ParseArguments<Options>(args)
            .MapResult(
                async opts => await RunOptions(opts, host.Services, logger),
                errs => Task.FromResult(1)
            );

        await host.StopAsync();

        stopwatch.Stop();
        logger.LogInformation("Total execution time: {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);

        Environment.Exit(await exitCode);
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                if (hostContext.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets<Program>();
                }
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
                    options.SingleLine = true;
                });
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogging(configure => configure.AddConsole());
                services.AddConfigurationModule(hostContext.Configuration);
                services.AddCommonServicesEngine(hostContext.Configuration);
                services.AddMediaSetOrganizer(hostContext.Configuration);
                services.AddInfuseMediaLibrary(hostContext.Configuration);
                services.AddPresentationAssetsBuilder(hostContext.Configuration);
                services.AddHealthCheck();
            });

    private static async Task<int> RunOptions(Options opts, IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        switch (opts.Workflow)
        {
            case HealthCheck.Workflow.WorkflowName:
                {
                    logger.LogInformation("Starting HealthCheck workflow.");
                    var workflow = scopedServices.GetRequiredService<HealthCheck.Workflow>();
                    var result = workflow.ExecuteAsync();
                    if (result.IsSuccess)
                    {
                        logger.LogInformation("HealthCheck workflow completed successfully.");
                        return 0; // success
                    }
                    else
                    {
                        logger.LogError("Error in HealthCheck workflow: {Error}", result.Error);
                        return 1; // error
                    }
                }

            case "FinalCutPro":
                {
                    logger.LogInformation("Starting FinalCutPro workflow.");
                    var workflow = scopedServices.GetRequiredService<MediaSetOrganizer.Workflow>();
                    var result = await workflow.ExecuteAsync();
                    if (result.IsSuccess)
                    {
                        logger.LogInformation("FinalCutPro workflow completed successfully.");
                        return 0; // success
                    }
                    else
                    {
                        logger.LogError("Error in FinalCutPro workflow: {Error}", result.Error);
                        return 1; // error
                    }
                }
            
            case InfuseMediaLibrary.Workflow.WorkflowName:
            {
                logger.LogInformation("Starting InfuseMediaLibrary workflow.");
                var workflow = scopedServices.GetRequiredService<InfuseMediaLibrary.Workflow>();
                var result = await workflow.ExecuteAsync();
                if (result.IsSuccess)
                {
                    logger.LogInformation("InfuseMediaLibrary workflow completed successfully.");
                    return 0; // success
                }
                else
                {
                    logger.LogError("Error in InfuseMediaLibrary workflow: {Error}", result.Error);
                    return 1; // error
                }
            }

            case MetadataXmlWorkflow.WorkflowName:
                {
                    logger.LogInformation("Starting MetadataXml workflow.");
                    var workflow = scopedServices.GetRequiredService<PresentationAssetsBuilder.MetadataXmlWorkflow>();
                    var result = await workflow.ExecuteAsync();
                    if (result.IsSuccess)
                    {
                        logger.LogInformation("MetadataXml workflow completed successfully.");
                        return 0; // success
                    }
                    else
                    {
                        logger.LogError("Error in MetadataXml workflow: {Error}", result.Error);
                        return 1; // error
                    }
                }

            case GenerateMediaSetIndexWorkflow.WorkflowName:
                {
                    logger.LogInformation("Starting GenerateMediaSetIndex workflow.");
                    var workflow = scopedServices.GetRequiredService<PresentationAssetsBuilder.GenerateMediaSetIndexWorkflow>();
                    var result = await workflow.ExecuteAsync();
                    if (result.IsSuccess)
                    {
                        logger.LogInformation("GenerateMediaSetIndex workflow completed successfully.");
                        return 0; // success
                    }
                    else
                    {
                        logger.LogError("Error in GenerateMediaSetIndex workflow: {Error}", result.Error);
                        return 1; // error
                    }
                }

            default:
                logger.LogWarning("No valid workflow specified.");
                return 1; // invalid workflow
        }
    }
}

public class Options
{
    [Option('w', "workflow", Required = true, HelpText = "Name of the workflow to execute.")]
    public string? Workflow { get; set; }

    [Option('e', "environment", Required = false, HelpText = "Environment to run.")]
    public string? Environment { get; set; }
}