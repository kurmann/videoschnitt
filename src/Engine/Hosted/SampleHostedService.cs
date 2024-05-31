using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.Engine.Hosted;

public class SampleHostedService : IHostedService, IDisposable
{
    private readonly ILogger<SampleHostedService> _logger;
    private readonly EngineSettings _settings;
    private Timer? _timer;

    public SampleHostedService(ILogger<SampleHostedService> logger, IOptions<EngineSettings> options)
    {
        _logger = logger;
        _settings = options.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sample Service is starting.");

        if (_settings.NewOriginalMediaDirectories != null)
        {
            foreach (var directory in _settings.NewOriginalMediaDirectories)
            {
                _logger.LogInformation("Configured directory to watch: {directory}", directory);
            }
        }

        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _logger.LogInformation("Sample Service is working. Current time: {time}", DateTimeOffset.Now);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sample Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}