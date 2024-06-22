using Microsoft.OpenApi.Models;
using System.Globalization;
using Kurmann.Videoschnitt.MetadataProcessor;
using Kurmann.Videoschnitt.Workflows;
using Kurmann.Videoschnitt.HealthCheck;
using Kurmann.Videoschnitt.HealthCheck.Services;
using Kurmann.Videoschnitt.InfuseMediaLibrary;

namespace Kurmann.Videoschnitt.Application;

public class Program
{
    public static void Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddMetadataProcessor(builder.Configuration);
        builder.Services.AddInfuseMediaLibrary(builder.Configuration);

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddHealthChecks();    
        builder.Services.AddControllers();
        builder.Services.AddSignalR();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Kurmann Videoschnitt API", Version = "v1" });
        });

        builder.Services.AddSingleton<LogHub>();
        builder.Services.AddScoped<FinalCutProWorkflow>();
        builder.Services.AddScoped<HealthCheckWorkflow>();
        builder.Services.AddScoped<HealthCheckFeature>();
        builder.Services.AddScoped<ToolsVersionService>();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kurmann Videoschnitt API v1"));

        // Minimal API Endpunkte
        app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy" }));

        app.MapGet("/api/startprocess", async (FinalCutProWorkflow workflow) =>
        {
            var result = await workflow.ExecuteAsync(new Progress<string>(_ => { }));
            if (result.IsSuccess)
            {
                return Results.Ok(new { status = "Process started successfully" });
            }
            else
            {
                return Results.BadRequest(new { status = "Process failed", error = result.Error });
            }
        });

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.MapHub<LogHub>("/logHub");

        app.MapHealthChecks("/health");

        app.Run();
    }
}