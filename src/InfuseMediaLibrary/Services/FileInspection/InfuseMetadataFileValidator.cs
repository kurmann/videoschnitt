using System.Xml.Linq;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.FileInspection;

public class InfuseMetadataFileValidator
{
    public static Result IsInfuseMetadataXmlFile(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            return IsInfuseMetadataXmlFile(fileInfo);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Fehler beim Überprüfen der Infuse-Metadaten-Datei: {ex.Message}");
        }
    }

    public static Result IsInfuseMetadataXmlFile(FileInfo? fileInfo)
    {
        // Prüfe, ob leerer Dateipfad übergeben wurde
        if (fileInfo == null)
        {
            return Result.Failure("Der Dateipfad ist leer.");
        }

        // Prüfe, ob die Datei existiert
        if (!fileInfo.Exists)
        {
            return Result.Failure("Die angegebene Datei wurde nicht gefunden.");
        }

        try
        {
            // Lade das XML-Dokument
            var document = XDocument.Load(fileInfo.FullName);

            // Prüfe, ob das Root-Element "media" ist und ob es das Attribut "type" mit dem Wert "Other" hat
            var mediaElement = document.Root;
            if (mediaElement == null || mediaElement.Name != "media" || mediaElement.Attribute("type")?.Value != "Other")
            {
                return Result.Failure("Die Datei ist keine gültige Infuse-Metadaten-Datei.");
            }

            // Prüfe, ob das Element "title" existiert und nicht leer ist
            var titleElement = mediaElement.Element("title");
            if (titleElement == null || string.IsNullOrWhiteSpace(titleElement.Value))
            {
                return Result.Failure("Der Titel in der Infuse-Metadaten-Datei ist ungültig oder fehlt.");
            }

            // Falls alle Prüfungen bestanden wurden
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Fehler beim Überprüfen der Infuse-Metadaten-Datei: {ex.Message}");
        }
    }
}
