using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;

public class Mpeg4Video : ISupportedMediaType
{
    public FileInfo FileInfo { get; }

    private Mpeg4Video(FileInfo fileInfo) => FileInfo = fileInfo;

    public static Result<Mpeg4Video> Create(FileInfo fileInfo)
    {
        // Prüfe, einschliesslich Gross- und Kleinschreibung (InvariantCultureIgnoreCase), ob die Dateiendung .mp4 oder .m4v ist
        if (fileInfo.Extension.Equals(".mp4", StringComparison.InvariantCultureIgnoreCase) || fileInfo.Extension.Equals(".m4v", StringComparison.InvariantCultureIgnoreCase))
        {
            return new Mpeg4Video(fileInfo);
        }

        return Result.Failure<Mpeg4Video>($"File {fileInfo.FullName} is not a supported MPEG-4 video.");
    }
}
