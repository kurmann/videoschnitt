using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.Application
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            try
            {
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

                if (args.Length > 0 && args[0] == "FinalCutPro")
                {
                    logger.LogInformation("Starting FinalCutPro workflow.");
                    // Hier kannst du den eigentlichen Workflow starten
                }
                else
                {
                    logger.LogInformation("No valid workflow specified.");
                }

                // Beenden, da keine langfristige Aufgabe erforderlich ist
                await host.StopAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while starting the application.");
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(configure => configure.AddConsole());
                    // Hier kannst du weitere Services hinzufügen, falls erforderlich
                });
    }
}