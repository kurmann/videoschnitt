using Microsoft.AspNetCore.SignalR;
using Kurmann.Videoschnitt.Features.MetadataProcessor.Events;

namespace Kurmann.Videoschnitt.Application;

public class MetadataProcessedEventLogHandler
{
    private readonly IHubContext<LogHub> _logHubContext;

    public MetadataProcessedEventLogHandler(IHubContext<LogHub> logHubContext)
    {
        _logHubContext = logHubContext;
    }

    public async Task Handle(MetadataProcessedEvent message)
    {
        var logMessage = $"Metadaten wurden erfolgreich verarbeitet: {message.Message}";
        await _logHubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);
        Console.WriteLine(logMessage);
    }
}
