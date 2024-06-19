using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;

public class QuickTimeMovie : ISupportedMediaType
{
    public FileInfo FileInfo { get; }

    private QuickTimeMovie(FileInfo fileInfo) => FileInfo = fileInfo;

    public static Result<QuickTimeMovie> Create(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Result.Failure<QuickTimeMovie>("The file path must not be empty.");
        }

        try 
        {
            var fileInfo = new FileInfo(filePath);
            return Create(fileInfo);
        }
        catch (Exception ex)
        {
            return Result.Failure<QuickTimeMovie>($"Fehler beim Erstellen des QuickTime-Movie-Objekts: {ex.Message}");
        }

    }

    public static Result<QuickTimeMovie> Create(FileInfo fileInfo)
    {

        // Prüfe, ob die Datei existiert
        if (!fileInfo.Exists)
        {
            return Result.Failure<QuickTimeMovie>($"Die Datei {fileInfo.FullName} existiert nicht.");
        }

        // Prüfe, ob die Dateierweiterung korrekt ist
        if (!IsQuickTimeMovieExtension(fileInfo))
        {
            return Result.Failure<QuickTimeMovie>($"Die Dateierweiterung {fileInfo.Extension} ist keine QuickTime-Movie-Datei.");
        }

        return new QuickTimeMovie(fileInfo);
    }

    private static bool IsQuickTimeMovieExtension(FileInfo fileInfo)
    {
        return fileInfo.Extension.Equals(".mov", StringComparison.InvariantCultureIgnoreCase) || fileInfo.Extension.Equals(".qt", StringComparison.InvariantCultureIgnoreCase);
    }
}
