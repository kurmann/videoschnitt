using Microsoft.AspNetCore.SignalR;
using Kurmann.Videoschnitt.Messages.Metadata;

namespace Kurmann.Videoschnitt.Application;

public class MetadataProcessedEventLogHandler(IHubContext<LogHub> logHubContext)
{
    public async Task Handle(MetadataProcessedEvent message)
    {
        var logMessage = $"Metadaten wurden erfolgreich verarbeitet: {message.Message}";
        await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);
        Console.WriteLine(logMessage);
    }
}
