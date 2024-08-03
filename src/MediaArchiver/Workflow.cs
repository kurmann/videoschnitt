using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MediaArchiver;

public interface IWorkflow
{
    const string WorkflowName = "MediaArchiver";

    Task<Result> ExecuteAsync();
}

/// <summary>
/// Archiviert Medien, vorwiegend Mediensets, in vorbestimmte Verzeichnisse für die Langzeitarchivierung.
/// </summary>
internal class Workflow : IWorkflow
{
    private readonly ILogger<Workflow> _logger;

    public Workflow(ILogger<Workflow> logger)
    {
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync()
    {
        _logger.LogInformation("Starting MediaArchiver workflow.");
        return Result.Success();
    }
}