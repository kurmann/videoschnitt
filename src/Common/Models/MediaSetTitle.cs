using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Common.Models;

/// <summary>
/// Repräsentiert ein Titel eines Mediensets bestehend aus dem ISO-Datum, Leerzeichen und dem restlichen Dateinamen
/// </summary>
public class MediaSetName
{
    public string Title { get; }
    public DateOnly Date { get; }

    private MediaSetName(string name, DateOnly date)
    {
        Title = name;
        Date = date;
    }

    public static Result<MediaSetName> Create(string name)
    {
        // Das ISO-Datum ist der erste Teil des Dateinamens und hat immer die gleiche Länge (10 Zeichen)
        var datePart = name[..10];

        // Versuche das ISO-Datum zu parsen
        if (!DateOnly.TryParse(datePart, out var date))
        {
            return Result.Failure<MediaSetName>($"Das Datum '{datePart}' konnte nicht geparst werden.");
        }

        // Der Teil nach dem Dateinamen und dem Leerzeichen ist der Titel des Mediensets
        var title = name[11..];

        return Result.Success(new MediaSetName(title, date));
    }
}
