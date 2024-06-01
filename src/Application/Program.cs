using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Kurmann.Videoschnitt.Application;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
        builder.WebHost.UseUrls($"http://*:{port}");

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        // app.UseHttpsRedirection(); // no certificate available for now

        app.UseStaticFiles();

        app.UseRouting();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
}