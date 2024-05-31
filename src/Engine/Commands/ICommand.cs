using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Engine.Commands;

public interface ICommand<T>
{
    Result<T> Execute();
}