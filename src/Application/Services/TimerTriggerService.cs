using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.Application.Services
{
    public class TimerTriggerService : IHostedService, IDisposable
    {
        private readonly ILogger<TimerTriggerService> _logger;
        private readonly IHubContext<LogHub> _hubContext;
        private Timer? _timer;
        private bool _isRunning;

        public TimerTriggerService(ILogger<TimerTriggerService> logger, IHubContext<LogHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timer Trigger Service is initialized.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timer Trigger Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            _isRunning = false;
            return Task.CompletedTask;
        }

        public void StartTimer()
        {
            if (_isRunning) return;
            _isRunning = true;

            _timer = new Timer(async (state) => await DoWork(state), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            _logger.LogInformation("Timer Trigger Service started on demand.");
        }

        public void StopTimer()
        {
            if (!_isRunning) return;
            _isRunning = false;

            _timer?.Change(Timeout.Infinite, 0);
            _logger.LogInformation("Timer Trigger Service stopped on demand.");
        }

        private async Task DoWork(object? state)
        {
            var now = DateTimeOffset.Now;
            _logger.LogInformation("Timer Trigger Service is working. Current time: {time}", now);
            await _hubContext.Clients.All.SendAsync("ReceiveLogMessage", $"Timer Trigger Service is working. Current time: {now}");
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}