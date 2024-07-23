using System.Globalization;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Common.Models;

/// <summary>
/// Repräsentiert ein den Namen eines MedienSets und validiert nach folgendem Schema: yyyy-MM-dd Titel
public class MediaSetName
{
    /// <summary>
    /// Der Titel des MedienSets. Entspricht in der Regel auch dem Titel des darin enthaltenen Videos.
    /// </summary>
    /// <value></value>
    public string Title { get; }

    /// <summary>
    /// Das Datum des MedienSets. Entspricht in den meisten Fällen dem Aufnahmedatum des darin enthaltenen Videos.
    /// </summary>
    /// <value></value>
    public DateOnly Date { get; }

    public string Name => $"{Date:yyyy-MM-dd} {Title}";

    private MediaSetName(string title, DateOnly date)
    {
        Title = title;
        Date = date;
    }

    public static Result<MediaSetName> Create(string name)
    {
        // The ISO date is the first part of the file name and always has the same length (10 characters)
        var datePart = name[..10];

        // Try to parse the ISO date
        if (!DateOnly.TryParseExact(datePart, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return Result.Failure<MediaSetName>($"Die Zeichenkette aus dem Medienset-Namen '{name}' konnte nicht als Datum interpretiert werden. Wahrscheinlich ist das Format des Datums nicht korrekt oder das Datum fehlt.");
        }

        // The part after the file name and the space is the title of the media set
        var title = name[11..];

        return Result.Success(new MediaSetName(title, date));
    }

    public override string ToString()
    {
        return Name;
    }

    public static implicit operator string(MediaSetName mediaSetTitle)
    {
        return mediaSetTitle.Name;
    }
}
