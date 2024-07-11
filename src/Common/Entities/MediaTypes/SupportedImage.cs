using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Common.Entities.MediaTypes;

public record SupportedImage : ISupportedMediaType
{
    /// <summary>
    /// Entspricht dem Dateipfad der Bilddatei.
    /// </summary>
    /// <value></value>
    public FileInfo FileInfo { get; private set; }

    /// <summary>
    /// Entspricht dem Dateipfad der gleichen Bilddatei konvertiert in den Adobe RGB-Farbraum (falls vorhanden).
    /// </summary>
    /// <value></value>
    public Maybe<FileInfo> FileInfoAdobeRgb { get; set; }

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

    public Result WithNewFilePath(string newFilePath)
    {
        try
        {
            var fileInfo = new FileInfo(newFilePath);
            FileInfo = fileInfo;
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure<SupportedImage>($"Error updating file path for {FileInfo.FullName}: {ex.Message}");
        }
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