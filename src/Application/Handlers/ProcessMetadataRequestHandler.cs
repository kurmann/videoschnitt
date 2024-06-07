using Microsoft.AspNetCore.SignalR;
using Kurmann.Videoschnitt.Messages.Metadata;

namespace Kurmann.Videoschnitt.Application;

public class ProcessMetadataRequestHandler(IHubContext<LogHub> logHubContext)
{
    public async Task Handle(ProcessMetadataRequest _)
    {
        var logMessage = "Anfrage zur Verarbeitung der Metadaten erhalten.";
        await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);
        Console.WriteLine(logMessage);
    }
}