using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Entities
{
    public class FFmpegMetadata
    {
        public string? MajorBrand { get; }
        public string? MinorVersion { get; }
        public string? CompatibleBrands { get; }
        public string? Copyright { get; }
        public DateOnly? Date { get; }
        public string? Keywords { get; }
        public string? Title { get; private set; }
        public string? Album { get; }
        public string? Artist { get; }
        public string? Author { get; }
        public string? DisplayName { get; }
        public DateOnly? CreationDate { get; }
        public string? Description { get; }
        public string? Encoder { get; }
        public DateOnly? PublishedDate { get; private set; }

        private FFmpegMetadata(Dictionary<string, string> metadata)
        {
            MajorBrand = metadata.GetValueOrDefault("major_brand");
            MinorVersion = metadata.GetValueOrDefault("minor_version");
            CompatibleBrands = metadata.GetValueOrDefault("compatible_brands");
            Copyright = metadata.GetValueOrDefault("com.apple.quicktime.copyright");
            Date = ParseDate(metadata.GetValueOrDefault("date"));
            Keywords = metadata.GetValueOrDefault("keywords");
            Title = metadata.GetValueOrDefault("title");
            Album = metadata.GetValueOrDefault("album");
            Artist = metadata.GetValueOrDefault("artist");
            Author = metadata.GetValueOrDefault("com.apple.quicktime.author");
            DisplayName = metadata.GetValueOrDefault("com.apple.quicktime.displayname");
            CreationDate = ParseDate(metadata.GetValueOrDefault("com.apple.quicktime.creationdate"));
            Description = metadata.GetValueOrDefault("com.apple.quicktime.description");
            Encoder = metadata.GetValueOrDefault("encoder");

            ParseTitleForDate();
        }

        public static Result<FFmpegMetadata> Create(string rawString)
        {
            if (string.IsNullOrWhiteSpace(rawString))
            {
                return Result.Failure<FFmpegMetadata>("The FFMpeg raw string is empty.");
            }

            var metadata = new Dictionary<string, string>();

            try
            {
                foreach (var line in rawString.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.StartsWith(";") || string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        metadata[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                return Result.Failure<FFmpegMetadata>($"Error while parsing FFmpeg metadata: {ex.Message}");
            }

            return new FFmpegMetadata(metadata);
        }

        private static DateOnly? ParseDate(string? dateString)
        {
            if (DateOnly.TryParse(dateString, out var date))
            {
                return date;
            }

            return null;
        }

        private void ParseTitleForDate()
        {
            if (string.IsNullOrEmpty(Title))
                return;

            var parts = Title.Split(' ', 2);
            if (parts.Length == 2 && DateOnly.TryParseExact(parts[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                PublishedDate = parsedDate;
                Title = parts[1];
            }
        }

        public XDocument ToInfuseXml()
        {
            var mediaElement = new XElement("media", new XAttribute("type", "Other"));

            if (!string.IsNullOrEmpty(Title))
                mediaElement.Add(new XElement("title", Title));
            
            if (!string.IsNullOrEmpty(Description))
                mediaElement.Add(new XElement("description", Description));
            
            if (!string.IsNullOrEmpty(Artist))
                mediaElement.Add(new XElement("artist", Artist));
            
            if (!string.IsNullOrEmpty(Copyright))
                mediaElement.Add(new XElement("copyright", Copyright));
            
            if (PublishedDate.HasValue)
                mediaElement.Add(new XElement("published", PublishedDate.Value.ToString("yyyy-MM-dd")));
            else if (Date.HasValue)
                mediaElement.Add(new XElement("published", Date.Value.ToString("yyyy-MM-dd")));

            if (CreationDate.HasValue)
                mediaElement.Add(new XElement("releasedate", CreationDate.Value.ToString("yyyy-MM-dd")));
            
            if (!string.IsNullOrEmpty(Author))
                mediaElement.Add(new XElement("studio", Author));
            
            if (!string.IsNullOrEmpty(Keywords))
                mediaElement.Add(new XElement("keywords", Keywords));
            
            if (!string.IsNullOrEmpty(Artist))
            {
                var producersElement = new XElement("producers");
                producersElement.Add(new XElement("name", Artist));
                mediaElement.Add(producersElement);
            }
            
            if (!string.IsNullOrEmpty(Artist))
            {
                var directorsElement = new XElement("directors");
                directorsElement.Add(new XElement("name", Artist));
                mediaElement.Add(directorsElement);
            }

            return new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), mediaElement);
        }
    }
}