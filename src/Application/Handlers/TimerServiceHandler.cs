using Kurmann.Videoschnitt.Application;
using Microsoft.AspNetCore.SignalR;

namespace Kurmann.Videoschnitt.Messages.Timer;

public class TimerServiceHandler
{
    private readonly IHubContext<LogHub> logHubContext;

    public TimerServiceHandler(IHubContext<LogHub> logHubContext)
    {
        this.logHubContext = logHubContext;
    }

    public async Task Handle(TimerTriggeredEvent timerTriggeredEvent)
    {
        var logMessage = $"Timer wurde ausgelöst: {timerTriggeredEvent.TriggeredAt}";
        await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);
        Console.WriteLine(logMessage);
    }
}