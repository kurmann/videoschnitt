using Kurmann.Videoschnitt.Features.MetadataProcessor;
using Microsoft.AspNetCore.SignalR;

namespace Kurmann.Videoschnitt.Application;

/// <summary>
/// Verantwortlich für die Verwaltung von Features
/// </summary>
public class FeatureService(MetadataProcessingService metadataProcessingService, IHubContext<LogHub> hubContext)
{
    private readonly MetadataProcessingService _metadataProcessingService = metadataProcessingService;
    private readonly IHubContext<LogHub> _hubContext = hubContext;

    /// <summary>
    /// Startet die Verarbeitung von Metadaten
    /// </summary>
    public async Task ProcessMetadataAsync()
    {
        _metadataProcessingService.MetadataProcessingEvent += async (sender, args) =>
        {
            await _hubContext.Clients.All.SendAsync("ReceiveLogMessage", args.Message);
        };

        await _metadataProcessingService.ProcessMetadataAsync();
    }

}