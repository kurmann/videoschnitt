using Kurmann.Videoschnitt.Engine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Kurmann.Videoschnitt.Kraftwerk.Application;

internal class Program
{
    static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
                {
                    config.AddUserSecrets<Program>();
                }
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddEngine(hostContext.Configuration);
            });
    }
}
