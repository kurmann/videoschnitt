using System.Text.Json;
using System.Xml.Linq;
using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Services.Metadata;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder.Entities;

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

    private CustomProductionInfuseMetadata(string type, string title, string? description, string? artist, string? copyright,
                        DateOnly? published, DateOnly? releaseDate, string? studio, string? keywords,
                        string? album, List<string> producers, List<string> directors)
    {
        Type = type;
        Title = title;
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
            var tags = format.GetProperty("tags");

            string type = "Other";
            string title = tags.GetProperty("title").GetString() ?? string.Empty;
            string description = tags.TryGetProperty("com.apple.quicktime.description", out var descProp) ? descProp.GetString() ?? string.Empty : string.Empty;
            string artist = tags.GetProperty("artist").GetString() ?? string.Empty;
            string copyright = tags.GetProperty("copyright").GetString() ?? string.Empty;

            DateOnly? published = recordingDate;
            DateOnly? releaseDate = DateOnly.TryParse(tags.GetProperty("com.apple.quicktime.creationdate").GetString(), out DateOnly releaseDateValue) ? releaseDateValue : null;
            string studio = tags.TryGetProperty("com.apple.quicktime.studio", out var studioProp) ? studioProp.GetString() ?? string.Empty : string.Empty;
            string keywords = tags.GetProperty("keywords").GetString() ?? string.Empty;
            string album = tags.GetProperty("album").GetString() ?? string.Empty;

            var producers = new List<string> { tags.GetProperty("producer").GetString() ?? string.Empty };
            var directors = new List<string>();

            return new CustomProductionInfuseMetadata(type, title, description, artist, copyright, published, releaseDate, studio, keywords, album, producers, directors);
        }
        catch (Exception ex)
        {
            return Result.Failure<CustomProductionInfuseMetadata>($"Fehler beim Parsen der FFprobe-Metadaten: {ex.Message}");
        }
    }

    public XElement ToXml()
    {
        return new XElement("media",
            new XAttribute("type", Type),
            new XElement("title", Title),
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