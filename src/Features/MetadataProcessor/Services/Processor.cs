using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.Features.MetadataProcessor.Services;

public class Processor : IHostedService, IDisposable
{
    private readonly ILogger<Processor> _logger;
    private readonly Settings _settings;
    private Timer? _timer;

    public Processor(ILogger<Processor> logger, IOptions<Settings> options)
    {
        _logger = logger;
        _settings = options.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Metadatenverarbeitungsdienst wird gestartet.");

        if (_settings.NewOriginalMediaDirectories != null)
        {
            foreach (var directory in _settings.NewOriginalMediaDirectories)
            {
                _logger.LogInformation("Folgende Verzeichnisse als Eingangsverzeichnisse definiert: {directory}", directory);
            }
        }

        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _logger.LogInformation("Metadatenverarbeitungsdienst ist aktiv. Aktuelle Zeit: {time}", DateTimeOffset.Now);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Metadatenverarbeitungsdienst wird gestoppt.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}