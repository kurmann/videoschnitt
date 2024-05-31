using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Engine.Commands;

public class SampleCommand(string? sampleParameter) : ICommand<SampleCommandResult>
{
    private readonly string? sampleParameter = sampleParameter;

    public Result<SampleCommandResult> Execute()
    {
        if (string.IsNullOrWhiteSpace(sampleParameter))
            return Result.Failure<SampleCommandResult>("Sample parameter cannot be empty");
    
        var commandResult = new SampleCommandResult(sampleParameter);

        return Result.Success(commandResult);
    }
}

public record SampleCommandResult(string Result);