using System.Xml.Linq;
using Kurmann.Videoschnitt.Common.Services.Metadata;

namespace Kurmann.Videoschnitt.PresentationAssetsBuilder.Entities;
    
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

        public static CustomProductionInfuseMetadata CreateFromFfmpegMetadata(FFmpegMetadata ffmpegMetadata)
        {
            var lines = ffmpegMetadata.Metadata
                .Where(line => !line.StartsWith(';'))
                .ToDictionary(line => line.Split('=')[0].Trim(), line => line.Split('=')[1].Trim());

            string type = "Other";
            string title = lines.GetValueOrDefault("title", string.Empty);
            string description = lines.GetValueOrDefault("com.apple.quicktime.description", string.Empty);
            string artist = lines.GetValueOrDefault("artist", string.Empty);
            string copyright = lines.GetValueOrDefault("copyright", string.Empty);

            DateOnly? published = DateOnly.TryParse(lines.GetValueOrDefault("date"), out DateOnly publishedDate) ? publishedDate : null;
            DateOnly? releaseDate = DateOnly.TryParse(lines.GetValueOrDefault("com.apple.quicktime.creationdate"), out DateOnly releaseDateValue) ? releaseDateValue : null;
            string studio = lines.GetValueOrDefault("com.apple.quicktime.studio", string.Empty);
            string keywords = lines.GetValueOrDefault("keywords", string.Empty);
            string album = lines.GetValueOrDefault("album", string.Empty);

            var producers = new List<string> { lines.GetValueOrDefault("producer", string.Empty) };
            var directors = new List<string>();

            return new CustomProductionInfuseMetadata(type, title, description, artist, copyright, published, releaseDate, studio, keywords, album, producers, directors);
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