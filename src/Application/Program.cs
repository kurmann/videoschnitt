using Microsoft.OpenApi.Models;
using Wolverine;
using System.Globalization;
using Kurmann.Videoschnitt.TimerService;
using Kurmann.Videoschnitt.MetadataProcessor;

namespace Kurmann.Videoschnitt.Application;

public class Program
{
    public static void Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseWolverine();

        var port = Environment.GetEnvironmentVariable("PORT") ?? "5024";
        builder.WebHost.UseUrls($"http://*:{port}");

                    // Add configuration sources
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>();

        builder.Services.AddMetadataProcessor(builder.Configuration);

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddHealthChecks();    
        builder.Services.AddControllers();
        builder.Services.AddSignalR();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Kurmann Videoschnitt API", Version = "v1"});
        });

        builder.Services.AddSingleton<LogHub>();

        builder.Services.AddTimerServices();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        // app.UseHttpsRedirection(); // no certificate available for now
        app.UseStaticFiles();
        app.UseRouting();
        app.UseSwagger();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kurmann Videoschnitt API v1"));

        // Minimal API Endpunkte
        app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy" }));

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        // SignalR Hub Endpunkt hinzufügen
        app.MapHub<LogHub>("/logHub");

        // Health Check Endpunkt hinzufügen
        app.MapHealthChecks("/health");

        app.Run();
    }
}