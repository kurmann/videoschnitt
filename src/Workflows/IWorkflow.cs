using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Workflows;

public interface IWorkflow
{
    Result Execute(IProgress<string> progress);
}