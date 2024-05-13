using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Kraftwerk.Queries;

public interface IQueryService<T>
{
    public Result<T> Execute();
}