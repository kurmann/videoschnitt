using System.Text.Json;
using System.Xml.Linq;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Services.Metadata;

namespace Kurmann.Videoschnitt.MediaSetOrganizer.Entities;

/// <summary>
/// Verantwortlich für das Erstellen von XML-Metadaten, die insbesonder von Infusse gelesen werden können und gleichzeitig auch allgemeine Metadaten enthalten.
/// Custom Production ist die Produktion von Videos, die nicht von einem Filmstudio stammen, bspw. private Videos.
/// Diese haben den Typ "Other".
/// Siehe auch: https://support.firecore.com/hc/en-us/articles/4405042929559-Overriding-Artwork-and-Metadata
/// </summary>
public class CustomProductionInfuseMetadata
{
    public string Type { get; }
    public string Title { get; }
    public string SortTitle { get; }
    public string? Description { get; }
    public string? Artist { get; }
    public string? Copyright { get; }
    public DateOnly? Published { get; }
    public DateOnly? ReleaseDate { get; }
    public string? Studio { get; }
    public string? Keywords { get; }
    public string? Album { get; }
    public List<string> Producers { get; }
    public List<string> Directors { get; }

    private CustomProductionInfuseMetadata(string type, string title, string sortTitle, string? description, string? artist, string? copyright,
                        DateOnly? published, DateOnly? releaseDate, string? studio, string? keywords,
                        string? album, List<string> producers, List<string> directors)
    {
        Type = type;
        Title = title;
        SortTitle = sortTitle;
        Description = description;
        Artist = artist;
        Copyright = copyright;
        Published = published;
        ReleaseDate = releaseDate;
        Studio = studio;
        Keywords = keywords;
        Album = album;
        Producers = producers;
        Directors = directors;
    }

    /// <summary>
    /// Erstellt ein CustomProductionInfuseMetadata-Objekt aus den Metadaten eines Videos, die von FFprobe im JSON-Format extrahiert wurden.
    /// Das Aufnahmedatum wird als Parameter übergeben, da es nicht in den Metadaten enthalten ist. Es wird zum XML-Tag "published" hinzugefügt.
    /// </summary>
    /// <param name="ffprobeJson"></param>
    /// <param name="recordingDate"></param>
    /// <returns></returns>
    public static Result<CustomProductionInfuseMetadata> CreateFromFfprobeMetadata(FFprobeMetadata fFprobeMetadata, DateOnly recordingDate)
    {
        if (fFprobeMetadata == null || fFprobeMetadata.Metadata == null)
        {
            return Result.Failure<CustomProductionInfuseMetadata>("Die FFprobe-Metadaten sind null.");
        }

        var json = fFprobeMetadata.ToString();

        // Parse JSON
        try
        {
            var document = JsonDocument.Parse(json);
            var format = document.RootElement.GetProperty("format");

            // Initialize variables with default values
            string type = "Other";
            string title = string.Empty;
            string sortTitle = string.Empty;
            string description = string.Empty;
            string artist = string.Empty;
            string copyright = string.Empty;
            DateOnly? releaseDate = null;
            string studio = string.Empty;
            string keywords = string.Empty;
            string album = string.Empty;
            var producers = new List<string> { string.Empty };
            var directors = new List<string>();

            if (format.TryGetProperty("tags", out JsonElement tags))
            {
                var titleWithLeadingDate = tags.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? string.Empty : string.Empty;
                title = GetTitle(titleWithLeadingDate, recordingDate);
                sortTitle = tags.TryGetProperty("title", out var sortTitleProp) ? sortTitleProp.GetString() ?? string.Empty : string.Empty; // Titel entspricht dem Namen des Mediensets bspw. "2022-01-01 - Titel"
                description = tags.TryGetProperty("com.apple.quicktime.description", out var descProp) ? descProp.GetString() ?? string.Empty : string.Empty;
                artist = tags.TryGetProperty("artist", out var artistProp) ? artistProp.GetString() ?? string.Empty : string.Empty;
                copyright = tags.TryGetProperty("copyright", out var copyrightProp) ? copyrightProp.GetString() ?? string.Empty : string.Empty;
                if (tags.TryGetProperty("com.apple.quicktime.creationdate", out var releaseDateProp))
                {
                    releaseDate = DateOnly.TryParse(releaseDateProp.GetString(), out DateOnly releaseDateValue) ? releaseDateValue : null;
                }
                studio = tags.TryGetProperty("com.apple.quicktime.studio", out var studioProp) ? studioProp.GetString() ?? string.Empty : string.Empty;
                keywords = tags.TryGetProperty("keywords", out var keywordsProp) ? keywordsProp.GetString() ?? string.Empty : string.Empty;
                album = tags.TryGetProperty("album", out var albumProp) ? albumProp.GetString() ?? string.Empty : string.Empty;
                producers = tags.TryGetProperty("producer", out var producerProp) ? new List<string> { producerProp.GetString() ?? string.Empty } : new List<string> { string.Empty };
                // You might need to handle multiple producers/directors if your metadata supports that
            }

            DateOnly? published = recordingDate;

            return new CustomProductionInfuseMetadata(type, title, sortTitle, description, artist, copyright, published, releaseDate, studio, keywords, album, producers, directors);
        }
        catch (Exception ex)
        {
            return Result.Failure<CustomProductionInfuseMetadata>($"Fehler beim Parsen der FFprobe-Metadaten: {ex.Message}");
        }
    }

    /// <summary>
    /// Gibt den eigentlichen Titel zurück. Entspricht dem Titel mit Datum am Anfang, wobei das Datum entfernt wird.
    /// </summary>
    /// <param name="titleWithLeadingDate"></param>
    /// <param name="recordingDate"></param>
    /// <returns></returns>
    private static string GetTitle(string titleWithLeadingDate, DateOnly recordingDate)
    {
        return titleWithLeadingDate.Replace(recordingDate.ToString("yyyy-MM-dd"),string.Empty).Trim();
    }

    public XElement ToXml()
    {
        return new XElement("media",
            new XAttribute("type", Type),
            new XElement("title", Title),
            new XElement("sorttitle", SortTitle),
            new XElement("description", Description),
            new XElement("artist", Artist),
            new XElement("copyright", Copyright),
            new XElement("published", Published?.ToString("yyyy-MM-dd")),
            new XElement("releasedate", ReleaseDate?.ToString("yyyy-MM-dd")),
            new XElement("studio", Studio),
            new XElement("keywords", Keywords),
            new XElement("album", Album),
            new XElement("producers", Producers.Select(p => new XElement("name", p))),
            new XElement("directors", Directors.Select(d => new XElement("name", d)))
        );
    }

    public override string ToString() => $"Title: {Title}, Published: {Published}, Album: {Album}";
}