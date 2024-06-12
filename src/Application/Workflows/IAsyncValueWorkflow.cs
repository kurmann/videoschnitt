using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Application.Workflows;

public interface IAsyncWorkflow<TResult>
{
    Task<Result<TResult>> ExecuteAsync();
}