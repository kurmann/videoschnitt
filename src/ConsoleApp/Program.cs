using System.Globalization;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Kurmann.Videoschnitt.Workflows;

namespace Kurmann.Videoschnitt.ConsoleApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(async opts => await RunOptions(opts, host.Services, logger))
            .WithNotParsed(errs => HandleParseError(errs, logger));

        await host.RunAsync();
    }

    private static async Task RunOptions(Options opts, IServiceProvider services, ILogger logger)
    {
        if (opts.Workflow == HealthCheckWorkflow.WorkflowName)
        {
            logger.LogInformation("Starting FinalCutPro workflow.");
            var workflow = services.GetRequiredService<HealthCheckWorkflow>();
            var result = workflow.Execute();
            if (result.IsSuccess)
            {
                logger.LogInformation("FinalCutPro workflow completed successfully.");
            }
            else
            {
                logger.LogError("Error in FinalCutPro workflow: {Error}", result.Error);
            }
        }
        // if (opts.Workflow == "FinalCutPro")
        // {
        //     logger.LogInformation("Starting FinalCutPro workflow.");
        //     var workflow = services.GetRequiredService<FinalCutProWorkflow>();
        //     var result = await workflow.ExecuteAsync();
        //     if (result.IsSuccess)
        //     {
        //         logger.LogInformation("FinalCutPro workflow completed successfully.");
        //     }
        //     else
        //     {
        //         logger.LogError("Error in FinalCutPro workflow: {Error}", result.Error);
        //     }
        // }
        else
        {
            logger.LogWarning("No valid workflow specified.");
        }
    }

    private static void HandleParseError(IEnumerable<Error> errs, ILogger logger)
    {
        // Handle errors
        logger.LogError("Error parsing arguments.");

        foreach (var error in errs)
        {
            logger.LogError("Error: {Error}", error);
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogging(configure => configure.AddConsole());
                services.AddScoped<HealthCheckWorkflow>();
            });
}

public class Options
{
    [Option('w', "workflow", Required = true, HelpText = "Name of the workflow to execute.")]
    public string? Workflow { get; set; }
}