using Kurmann.Videoschnitt.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("api/[controller]")]
public class LogController(IHubContext<LogHub> hubContext) : ControllerBase
{
    private readonly IHubContext<LogHub> _hubContext = hubContext;

    [HttpPost]
    public async Task<IActionResult> SendLog(string message)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveLog", message);
        return Ok();
    }
}