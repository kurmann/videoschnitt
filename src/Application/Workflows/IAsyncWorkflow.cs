using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Application.Workflows;

public interface IAsyncWorkflow
{
    Task<Result> ExecuteAsync();
}