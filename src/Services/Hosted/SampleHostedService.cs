using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.Kraftwerk.Hosted;

public class SampleHostedService(ILogger<SampleHostedService> logger, IOptionsSnapshot<KraftwerkSettings> options) : IHostedService, IDisposable
{
    private readonly ILogger<SampleHostedService> _logger = logger;
    private readonly KraftwerkSettings _options = options.Value;
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sample Service is starting.");

        if (string.IsNullOrEmpty(_options.SampleSetting))
        {
            _logger.LogWarning("SampleSetting is not set in configuration.");
        }
        else
        {
            _logger.LogInformation("SampleSetting has been successfully loaded from configuration");
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