using System.Diagnostics;
using System.Globalization;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Kurmann.Videoschnitt.Workflows;
using Kurmann.Videoschnitt.ConfigurationModule;
using Kurmann.Videoschnitt.ConfigurationModule.Services;

namespace Kurmann.Videoschnitt.ConsoleApp
{
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
                    config.AddUserSecrets<Program>();
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
                    services.AddWorkflows(hostContext.Configuration);
                });

        private static async Task<int> RunOptions(Options opts, IServiceProvider services, ILogger logger)
        {
            if (opts.Workflow == HealthCheckWorkflow.WorkflowName)
            {
                logger.LogInformation("Starting HealthCheck workflow.");
                var workflow = services.GetRequiredService<HealthCheckWorkflow>();
                var result = workflow.Execute();
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
            else if (opts.Workflow == "FinalCutPro")
            {
                logger.LogInformation("Starting FinalCutPro workflow.");
                var workflow = services.GetRequiredService<FinalCutProWorkflow>();
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
            else
            {
                logger.LogWarning("No valid workflow specified.");
                return 1; // invalid workflow
            }
        }
    }

    public class Options
    {
        [Option('w', "workflow", Required = true, HelpText = "Name of the workflow to execute.")]
        public string? Workflow { get; set; }
    }
}