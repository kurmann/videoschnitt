using Microsoft.OpenApi.Models;
using Wolverine;
using System.Globalization;
using Kurmann.Videoschnitt.Features.MetadataProcessor;

namespace Kurmann.Videoschnitt.Application;

public class Program
{
    public static void Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseWolverine(opts =>
        {
            // Fügen Sie so viele andere Assemblys hinzu, wie Sie benötigen
            opts.Discovery.IncludeAssembly(typeof(MetadataProcessingService).Assembly);
        });

        var port = Environment.GetEnvironmentVariable("PORT") ?? "5024";
        builder.WebHost.UseUrls($"http://*:{port}");

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