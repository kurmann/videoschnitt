using Microsoft.AspNetCore.SignalR;
using Kurmann.Videoschnitt.Messages.Metadata;

namespace Kurmann.Videoschnitt.Application;

public class ProcessMetadataRequestHandler
{
    private readonly IHubContext<LogHub> _logHubContext;

    public ProcessMetadataRequestHandler(IHubContext<LogHub> logHubContext)
    {
        _logHubContext = logHubContext;
    }

    public async Task Handle(ProcessMetadataRequest _)
    {
        var logMessage = "Anfrage zur Verarbeitung der Metadaten erhalten.";
        await _logHubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);
        Console.WriteLine(logMessage);
    }
}