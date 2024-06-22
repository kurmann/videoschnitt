using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.LocalFileSystem.Services.ImageProcessing;

public interface IColorConversionService
{
    Task<Result> ConvertColorSpaceAsync(string inputFilePath, string outputFilePath, string inputColorSpace, string outputColorSpace);
}
