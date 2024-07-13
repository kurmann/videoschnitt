using System.Globalization;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Common.Models;

/// <summary>
/// Repräsentiert ein Titel eines Mediensets bestehend aus dem ISO-Datum, Leerzeichen und dem restlichen Dateinamen
/// </summary>
public class MediaSetTitle
{
    public string Title { get; }
    public DateOnly Date { get; }

    public string Name => $"{Date:yyyy-MM-dd} {Title}";

    private MediaSetTitle(string title, DateOnly date)
    {
        Title = title;
        Date = date;
    }

    public static Result<MediaSetTitle> Create(string name)
    {
        // The ISO date is the first part of the file name and always has the same length (10 characters)
        var datePart = name[..10];

        // Try to parse the ISO date
        if (!DateOnly.TryParseExact(datePart, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return Result.Failure<MediaSetTitle>($"The date '{datePart}' could not be parsed.");
        }

        // The part after the file name and the space is the title of the media set
        var title = name[11..];

        return Result.Success(new MediaSetTitle(title, date));
    }

    public override string ToString()
    {
        return Name;
    }

    public static implicit operator string(MediaSetTitle mediaSetTitle)
    {
        return mediaSetTitle.Name;
    }
}
