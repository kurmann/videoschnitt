using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Common.Entities.MediaTypes;

public record SupportedImage : ISupportedMediaType
{
    public FileInfo FileInfo { get; }

    public bool IsTiffOrPng => FileInfo.Extension.Equals(".tiff", StringComparison.InvariantCultureIgnoreCase) || 
        FileInfo.Extension.Equals(".tif", StringComparison.InvariantCultureIgnoreCase) ||
        FileInfo.Extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase);

    public bool IsJpeg => FileInfo.Extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase) || 
        FileInfo.Extension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase);

    public bool IsAdobeRgbColorSpace { get; private set; }

    private SupportedImage(FileInfo fileInfo, bool isAdobeRgbColorSpace)
    {
        FileInfo = fileInfo;
        IsAdobeRgbColorSpace = isAdobeRgbColorSpace;
    }

    public static Result<SupportedImage> Create(FileInfo fileInfo, bool isAdobeRgbColorSpace = false)
    {
        // Prüfe, einschliesslich Gross- und Kleinschreibung (InvariantCultureIgnoreCase), ob die Dateiendung .jpg, .jpeg oder .png ist
        if (IsSupportedImageExtension(fileInfo))
        {
            return new SupportedImage(fileInfo, isAdobeRgbColorSpace);
        }

        return Result.Failure<SupportedImage>($"File {fileInfo.FullName} is not a supported cover art image.");
    }

    public static Result<SupportedImage> CreateWithUpdatedFilePath(SupportedImage supportedImage, string newFilePath)
    {
        return Create(new FileInfo(newFilePath), supportedImage.IsAdobeRgbColorSpace);
    }

    public static Result<SupportedImage> Create(string directory, string fileName)
    {
        try
        {
            return Create(new FileInfo(Path.Combine(directory, fileName)));
        }
        catch (Exception ex)
        {
            return Result.Failure<SupportedImage>($"Error creating SupportedImage from directory {directory} and file name {fileName}: {ex.Message}");
        }
    }

    public static bool IsSupportedImageExtension(FileInfo fileInfo)
    {
        return fileInfo.Extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase) ||
            fileInfo.Extension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase) ||
            fileInfo.Extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase) ||
            fileInfo.Extension.Equals(".tiff", StringComparison.InvariantCultureIgnoreCase) ||
            fileInfo.Extension.Equals(".tif", StringComparison.InvariantCultureIgnoreCase);
    }

    public override string ToString() => FileInfo.Name;
}