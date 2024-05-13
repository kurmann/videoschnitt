using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Kraftwerk.Queries;

public class SampleQuery(string? sampleParameter) : IQueryService<SampleQueryResult>
{
    private readonly string? sampleParameter = sampleParameter;

    public Result<SampleQueryResult> Execute()
    {
        if (string.IsNullOrWhiteSpace(sampleParameter))
            return Result.Failure<SampleQueryResult>("Sample parameter cannot be empty");

        var sampleEntity = new SampleQueryResult(sampleParameter);

        return Result.Success(sampleEntity);
    }
}

public record SampleQueryResult(string Result);