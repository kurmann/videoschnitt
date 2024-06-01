using Microsoft.AspNetCore.SignalR;

namespace Kurmann.Videoschnitt.Application;
public class LogHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveLogMessage", message);
    }
}