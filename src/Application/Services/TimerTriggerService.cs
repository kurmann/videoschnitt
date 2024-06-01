using Microsoft.AspNetCore.SignalR;

namespace Kurmann.Videoschnitt.Application.Services
{
    public class TimerTriggerService(ILogger<TimerTriggerService> logger, IHubContext<LogHub> hubContext) : IHostedService, IDisposable
    {
        private readonly ILogger<TimerTriggerService> _logger = logger;
        private readonly IHubContext<LogHub> _hubContext = hubContext;
        private Timer? _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timer Trigger Service is starting.");

            _timer = new Timer(async (state) => await DoWork(state), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private async Task DoWork(object? state)
        {
            var now = DateTimeOffset.Now;
            _logger.LogInformation("Timer Trigger Service is working. Current time: {time}", now);
            await _hubContext.Clients.All.SendAsync("ReceiveLogMessage", $"Timer Trigger Service is working. Current time: {now}");
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
            GC.SuppressFinalize(this);
        }
    }
}