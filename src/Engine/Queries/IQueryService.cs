using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Engine.Queries;

public interface IQueryService<T>
{
    public Result<T> Execute();
}