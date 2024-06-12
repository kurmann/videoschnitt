using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Entities;

/// <summary>
/// Repräsentiert Informationen über einen Dateinamen, ohne direkt vom Dateisystem abhängig zu sein. Diese Klasse ist unveränderlich.
/// </summary>
public class FileNameInfo
{
    /// <summary>
    /// Der Dateiname mit Dateierweiterung.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Der Dateiname ohne Dateierweiterung.
    /// </summary>
    public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FileName);

    /// <summary>
    /// Die Dateierweiterung.
    /// </summary>
    public string Extension => Path.GetExtension(FileName);

    private FileNameInfo(string fileName) => FileName = Path.GetFileName(fileName);

    /// <summary>
    /// Erstellt ein FileNameInfo-Objekt.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static Result<FileNameInfo> Create(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Result.Failure<FileNameInfo>("File name is null or empty");
        }

        // Prüfe, ob der Dateiname nur ein Verzeichnis ist
        if (!Path.HasExtension(fileName))
        {
            return Result.Failure<FileNameInfo>("File name is not a file");
        }

        // Entferne den Pfad, falls vorhanden
        fileName = Path.GetFileName(fileName);

        // Prüfe auf unzulässige Zeichen im Dateinamen
        if (CrossPlatformInvalidCharsHandler.ContainsInvalidChars(fileName))
        {
            return Result.Failure<FileNameInfo>("File name contains invalid characters: " + string.Join(", ", CrossPlatformInvalidCharsHandler.InvalidChars));
        }

        return Result.Success(new FileNameInfo(fileName));
    }

    public override string ToString() => FileName;

    public static implicit operator string(FileNameInfo fileNameInfo) => fileNameInfo.FileName;

    /// <summary>
    /// Verwaltet ungültige Zeichen für Datei- und Verzeichnisnamen.
    /// Im Gegegensatz zu Path.GetInvalidPathChars() und Path.GetInvalidFileNameChars() enthält diese Klasse
    /// ungültige Zeichen für alle Windows- und Unix-Dateisysteme.
    /// Unter Windows sind die ungültigen Zeichen für Datei- und Verzeichnisnamen in der Regel:
    /// < (Kleiner als)
    /// > (Größer als)
    /// : (Doppelpunkt)
    /// " (Anführungszeichen)
    /// / (Schrägstrich)
    /// \ (Rückwärtsschrägstrich)
    /// | (Senkrechter Strich)
    /// ? (Fragezeichen)
    /// * (Sternchen)
    /// Zusätzlich sind Zeichen mit ASCII-Werten von 0 bis 31 (Steuerzeichen) ebenfalls ungültig.
    /// </summary>
    private class CrossPlatformInvalidCharsHandler
    {
        public static readonly List<char> InvalidChars;
        public static readonly List<char> InvalidCharsForWindowsPaths = new List<char>{'\\', ':', '*', '?', '"', '<', '>', '|'};
        public static readonly List<char> InvalidCharsForUnixPaths = new List<char>{'/'};

        static CrossPlatformInvalidCharsHandler()
        {
            // Gemeinsam für Datei- und Verzeichnisnamen
            InvalidChars = new List<char>
            {
                '<', '>', ':', '"', '|', '?', '*',
            };

            for (int i = 0; i < 32; i++) // Steuerzeichen
            {
                InvalidChars.Add((char)i);
            }

            // '/' fügen wir nicht zu InvalidChars hinzu, da es in Unix-Pfaden gültig ist
        }

        /// <summary>
        /// Gibt zurück, ob der gegebene Datei- oder Verzeichnisname ungültige Zeichen enthält.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool ContainsInvalidChars(string name)
        {
            return InvalidChars.Any(name.Contains);
        }

        /// <summary>
        /// Gibt zurück, ob der gegebene Pfad ungültige Zeichen enthält für Windows-Dateisysteme.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool ContainsInvalidPathCharsInWindowsPath(string path)
        {
            var invalidChars = InvalidCharsForWindowsPaths;
            return invalidChars.Any(path.Contains);
        }

        /// <summary>
        /// Gibt zurück, ob der gegebene Pfad ungültige Zeichen enthält für Unix-Dateisysteme.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool ContainsInvalidPathCharsInUnixPath(string path)
        {
            var invalidChars = InvalidCharsForUnixPaths;
            return invalidChars.Any(path.Contains);
        }

        public static string ReplaceInvalidChars(string name, char replacementChar)
        {
            foreach (char c in InvalidChars)
            {
                name = name.Replace(c, replacementChar);
            }
            return name;
        }

        public static string ReplaceInvalidPathChars(string path, char replacementChar, bool isWindowsPath)
        {
            var invalidChars = isWindowsPath ? InvalidCharsForWindowsPaths : InvalidCharsForUnixPaths;
            foreach (char c in invalidChars)
            {
                path = path.Replace(c, replacementChar);
            }
            return path;
        }
    }
}