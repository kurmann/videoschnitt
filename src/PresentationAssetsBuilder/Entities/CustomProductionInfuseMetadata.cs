using System.Xml.Linq;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder.Entities;

public class CustomProductionInfuseMetadata
{
    public string Type { get; }
    public string Title { get; }
    public string? Description { get; }
    public string? Artist { get; }
    public string? Copyright { get; }

    /// <summary>
    /// Veröffentlichungsdatum. Entspricht bei den Eigenproduktionen das Aufnamedatum.
    /// </summary>
    public DateOnly? Published { get; }

    /// <summary>
    /// Veröffentlichungsdatum. Entspricht bei den Eigenproduktionen dem Tag des Videoschnitts.
    /// </summary>
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

    public static Result<CustomProductionInfuseMetadata> Create(string xmlContent)
    {
        try
        {
            XDocument doc = XDocument.Parse(xmlContent);

            XElement? mediaElement = doc.Element("media");

            // Prüfe, ob ein <media>-Element vorhanden ist
            if (mediaElement == null)
            {
                return Result.Failure<CustomProductionInfuseMetadata>("Inkorrektes Infuse Metadata-XML: <media>-Element fehlt.");
            }

            // Prüfe, ob ein Type-Attribut vorhanden ist
            if (mediaElement.Attribute("type") == null || mediaElement.Attribute("type")!.Value == null)
            {
                return Result.Failure<CustomProductionInfuseMetadata>("Inkorrektes Infuse Metadata-XML: Das Type-Attribut des <media>-Elements muss 'Other' sein.");
            }

            if (mediaElement.Attribute("type")?.Value != "Other")
            {
                return Result.Failure<CustomProductionInfuseMetadata>("Inkorrektes Infuse Metadata-XML: Das Type-Attribut des <media>-Elements muss 'Other' sein.");
            }
            string type = mediaElement.Attribute("type")!.Value;


            XElement? titleElement = mediaElement.Element("title");
            if (titleElement == null)
            {
                return Result.Failure<CustomProductionInfuseMetadata>("Inkorrektes Infuse Metadata-XML: <title>-Element fehlt.");
            }

            string? title = titleElement.Value;
            string? description = mediaElement.Element("description")?.Value;
            string? artist = mediaElement.Element("artist")?.Value;
            string? copyright = mediaElement.Element("copyright")?.Value;
            DateOnly? published = DateOnly.TryParse(mediaElement.Element("published")?.Value, out DateOnly publishedDate) ? publishedDate : null;
            DateOnly? releaseDate = DateOnly.TryParse(mediaElement.Element("releasedate")?.Value, out DateOnly releaseDateValue) ? releaseDateValue : null;
            string? studio = mediaElement.Element("studio")?.Value;
            string? keywords = mediaElement.Element("keywords")?.Value;
            string? album = mediaElement.Element("album")?.Value;

            var producers = mediaElement.Element("producers")?.Elements("name").Select(e => e.Value).ToList() ?? new List<string>();
            var directors = mediaElement.Element("directors")?.Elements("name").Select(e => e.Value).ToList() ?? new List<string>();

            var mediaEntity = new CustomProductionInfuseMetadata(type, title, description, artist, copyright, published, releaseDate, studio, keywords, album, producers, directors);
            return Result.Success(mediaEntity);
        }
        catch (Exception ex)
        {
            return Result.Failure<CustomProductionInfuseMetadata>($"Fehler beim Interpretieren des Infuse Metadata-XML: {ex.Message}");
        }
    }

    public override string ToString() => $"Title: {Title}, Published: {Published}, Album: {Album}";
}