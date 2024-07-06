using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Workflows.Abstractions;

/// <summary>
/// Repräsentiert einen asynchronen Workflow, der keinen Wert zurückgibt.
/// </summary>
public interface IAsyncWorkflow
{
    Task<Result> ExecuteAsync();
}