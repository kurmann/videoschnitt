using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Application.Workflows.Models;

namespace Kurmann.Videoschnitt.Application.Workflows;

/// <summary>
/// Repräsentiert einen asynchronen Workflow, der einen Wert zurückgibt.
/// </summary>
public interface IAsyncWorkflow<TResult>
{
    Task<Result<TResult>> ExecuteAsync(Action<StatusUpdate> statusCallback);
}