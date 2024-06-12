using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Workflows;

/// <summary>
/// Repräsentiert einen asynchronen Workflow, der einen Wert zurückgibt.
/// </summary>
public interface IAsyncWorkflow<TResult>
{
    Task<Result<TResult>> ExecuteAsync(IProgress<string> progress);
}