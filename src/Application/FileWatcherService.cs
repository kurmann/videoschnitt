using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.Application;

public class FileWatcherService(ILogger<FileWatcherService> logger) : IHostedService, IDisposable
{
    private readonly ILogger<FileWatcherService> _logger = logger;
    private FileSystemWatcher? _fileSystemWatcher;
    private readonly string? _path = Environment.GetEnvironmentVariable("LOCAL_MEDIA_LIBRARY_PATH");

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("File Watcher Service starting...");

        if (string.IsNullOrEmpty(_path))
        {
            _logger.LogError("Path not specified in environment variables");
            return Task.CompletedTask;
        }

        _fileSystemWatcher = new FileSystemWatcher(_path)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            Filter = "*.*"
        };

        _fileSystemWatcher.Changed += OnChanged;
        _fileSystemWatcher.Created += OnCreated;
        _fileSystemWatcher.Deleted += OnDeleted;
        _fileSystemWatcher.Renamed += OnRenamed;
        _fileSystemWatcher.EnableRaisingEvents = true;

        _logger.LogInformation($"Started watching {_path}");

        return Task.CompletedTask;
    }

    private void OnCreated(object source, FileSystemEventArgs e) =>
        _logger.LogInformation($"File: {e.FullPath} {e.ChangeType}");

    private void OnDeleted(object source, FileSystemEventArgs e) =>
        _logger.LogInformation($"File: {e.FullPath} {e.ChangeType}");

    private void OnChanged(object source, FileSystemEventArgs e) =>
        _logger.LogInformation($"File: {e.FullPath} {e.ChangeType}");

    private void OnRenamed(object source, RenamedEventArgs e) =>
        _logger.LogInformation($"File: {e.OldFullPath} renamed to {e.FullPath}");

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("File Watcher Service stopping...");

        if (_fileSystemWatcher == null)
        {
            return Task.CompletedTask;
        }
        _fileSystemWatcher.EnableRaisingEvents = false;
        _fileSystemWatcher.Dispose();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _fileSystemWatcher?.Dispose();
    }
}