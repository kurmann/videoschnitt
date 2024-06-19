using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;

public class Mpeg4Video : ISupportedMediaType
{
    public FileInfo FileInfo { get; }

    private Mpeg4Video(FileInfo fileInfo) => FileInfo = fileInfo;

    public static Result<Mpeg4Video> Create(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Result.Failure<Mpeg4Video>($"Der Dateipfad darf nicht leer sein.");
        }

        var fileInfo = new FileInfo(filePath);
        return Create(fileInfo);
    }

    public static Result<Mpeg4Video> Create(FileInfo fileInfo)
    {
        if (!fileInfo.Exists)
        {
            return Result.Failure<Mpeg4Video>($"Die Datei {fileInfo.FullName} existiert nicht.");
        }

        // Prüfe, einschliesslich Gross- und Kleinschreibung (InvariantCultureIgnoreCase), ob die Dateiendung .mp4 oder .m4v ist
        var isMpeg4 = IsVideoExtensionMpeg4(fileInfo);
        if (isMpeg4)
        {
            return new Mpeg4Video(fileInfo);
        }

        return Result.Failure<Mpeg4Video>($"Die Dateierweiterung {fileInfo.Extension} ist keine MPEG4-Datei.");
    }

    private static bool IsVideoExtensionMpeg4(FileInfo fileInfo)
    {
        return fileInfo.Extension.Equals(".mp4", StringComparison.InvariantCultureIgnoreCase) || fileInfo.Extension.Equals(".m4v", StringComparison.InvariantCultureIgnoreCase);
    }
}
