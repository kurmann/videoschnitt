using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Kraftwerk.Commands;

public interface ICommand<T>
{
    Result<T> Execute();
}