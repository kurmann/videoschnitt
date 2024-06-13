using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Entities;

public class FFmpegMetadata
{
    public string? MajorBrand { get; }
    public string? MinorVersion { get; }
    public string? CompatibleBrands { get; }
    public string? Copyright { get; }
    public DateOnly? Date { get; }
    public string? Keywords { get; }
    public string? Title { get; }
    public string? Album { get; }
    public string? Artist { get; }
    public string? Author { get; }
    public string? DisplayName { get; }
    public DateOnly? CreationDate { get; }
    public string? Description { get; }
    public string? Encoder { get; }

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
    }

    public static Result<FFmpegMetadata> Create(string rawString)
    {
        // Prüfen, ob die Zeichenfolge leer ist
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
        catch (System.Exception ex)
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
}
