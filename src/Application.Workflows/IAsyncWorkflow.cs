using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Application.Workflows.Models;

namespace Kurmann.Videoschnitt.Application.Workflows;

/// <summary>
/// Repräsentiert einen asynchronen Workflow, der keinen Wert zurückgibt.
/// </summary>
public interface IAsyncWorkflow
{
    Task<Result> ExecuteAsync(IProgress<string> progress);
}