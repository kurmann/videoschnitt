using Kurmann.Videoschnitt.Application;
using Microsoft.AspNetCore.SignalR;

namespace Kurmann.Videoschnitt.Messages.Timer;

public class ProcessMetadataRequestHandler(IHubContext<LogHub> logHubContext)
{
    public async Task Handle(TimerTriggeredEvent timerTriggeredEvent)
    {
        var logMessage = $"Timer wurde ausgelöst: {timerTriggeredEvent.TriggeredAt}";
        await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);
        Console.WriteLine(logMessage);
    }
}