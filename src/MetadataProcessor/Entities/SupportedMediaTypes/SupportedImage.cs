using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;

public record SupportedImage : ISupportedMediaType
{
    public FileInfo FileInfo { get; }

    private SupportedImage(FileInfo fileInfo) => FileInfo = fileInfo;

    public static Result<SupportedImage> Create(FileInfo fileInfo)
    {
        // Prüfe, einschliesslich Gross- und Kleinschreibung (InvariantCultureIgnoreCase), ob die Dateiendung .jpg, .jpeg oder .png ist
        if (IsSupportedImageExtension(fileInfo))
        {
            return new SupportedImage(fileInfo);
        }

        return Result.Failure<SupportedImage>($"File {fileInfo.FullName} is not a supported cover art image.");
    }

    public static bool IsSupportedImageExtension(FileInfo fileInfo)
    {
        return fileInfo.Extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase) || fileInfo.Extension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase) || fileInfo.Extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase);
    }

    public override string ToString() => FileInfo.Name;
}