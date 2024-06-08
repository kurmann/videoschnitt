using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wolverine;
using Kurmann.Videoschnitt.Messages.Timer;

namespace Kurmann.Videoschnitt.TimerService.Services;

public class TimerTriggerService :  IHostedService, IDisposable
{
    private readonly ILogger<TimerTriggerService> _logger;
    private Timer? _timer;
    private IMessageBus _bus;
    private const int IntervalInSeconds = 5;

    public TimerTriggerService(ILogger<TimerTriggerService> logger, IMessageBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timer Trigger Service is starting.");
        _timer = new Timer((state) => DoWork(state), null, TimeSpan.Zero, TimeSpan.FromSeconds(IntervalInSeconds));
        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        var now = DateTimeOffset.Now;
        var timerEvent = new TimerTriggeredEvent(now, TimeSpan.FromSeconds(IntervalInSeconds));
        await _bus.PublishAsync(timerEvent);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timer Trigger Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}