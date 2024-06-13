using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;

public record CoverArtImage : ISupportedMediaType
{
    public FileInfo FileInfo { get; }

    private CoverArtImage(FileInfo fileInfo) => FileInfo = fileInfo;

    public static Result<CoverArtImage> Create(FileInfo fileInfo)
    {
        // Prüfe, einschliesslich Gross- und Kleinschreibung (InvariantCultureIgnoreCase), ob die Dateiendung .jpg, .jpeg oder .png ist
        if (fileInfo.Extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase) || fileInfo.Extension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase) || fileInfo.Extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase))
        {
            return new CoverArtImage(fileInfo);
        }

        return Result.Failure<CoverArtImage>($"File {fileInfo.FullName} is not a supported cover art image.");
    }
}