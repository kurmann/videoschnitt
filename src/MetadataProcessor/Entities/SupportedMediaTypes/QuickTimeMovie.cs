using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;

public class QuickTimeMovie : ISupportedMediaType
{
    public FileInfo FileInfo { get; }

    private QuickTimeMovie(FileInfo fileInfo) => FileInfo = fileInfo;

    public static Result<QuickTimeMovie> Create(FileInfo fileInfo)
    {
        // Prüfe, einschliesslich Gross- und Kleinschreibung (InvariantCultureIgnoreCase), ob die Dateiendung .mov oder .qt ist
        if (fileInfo.Extension.Equals(".mov", StringComparison.InvariantCultureIgnoreCase) || fileInfo.Extension.Equals(".qt", StringComparison.InvariantCultureIgnoreCase))
        {
            return new QuickTimeMovie(fileInfo);
        }

        return Result.Failure<QuickTimeMovie>($"File {fileInfo.FullName} is not a supported QuickTime movie.");
    }
}
