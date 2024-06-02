using Kurmann.Videoschnitt.Features.MetadataProcessor;
using Microsoft.AspNetCore.SignalR;

namespace Kurmann.Videoschnitt.Application;

/// <summary>
/// Verantwortlich für die Verwaltung von Features
/// </summary>
public class FeatureService
{
    private readonly MetadataProcessingService _metadataProcessingService;
    private readonly IHubContext<LogHub> _hubContext;

    public FeatureService(MetadataProcessingService metadataProcessingService, IHubContext<LogHub> hubContext)
    {
        _metadataProcessingService = metadataProcessingService;
        _hubContext = hubContext;

        _metadataProcessingService.MetadataProcessingEvent += async (sender, args) =>
        {
            await _hubContext.Clients.All.SendAsync("ReceiveLogMessage", args.Message);
        };
    }

    public async Task ProcessMetadataAsync()
    {
        await _metadataProcessingService.ProcessMetadataAsync();
    }
}