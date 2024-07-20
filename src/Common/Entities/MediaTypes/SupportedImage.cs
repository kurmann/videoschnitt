using System.Reflection;
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
    /// Der Dateiname der Bilddatei (inkl. Dateiendung und ohne Pfad).
    /// </summary>
    public string Name => FileInfo.Name;

    /// <summary>
    /// Der vollständige Dateipfad der Bilddatei.
    /// </summary>
    public string FullName => FileInfo.FullName;

    /// <summary>
    /// Gibt die Dateiendung der Originaldatei zurück.
    /// </summary>
    public string Extension => FileInfo.Extension;

    /// <summary>
    /// Gibt die Dateiendung der Bilddatei im Adobe RGB-Farbraum zurück (falls vorhanden).
    /// </summary>
    public Maybe<string> ExtensionAdobeRgb => FileInfoAdobeRgb.HasValue ? FileInfoAdobeRgb.Value.Extension : Maybe<string>.None;

    /// <summary>
    /// Entspricht dem Dateipfad der gleichen Bilddatei konvertiert in den Adobe RGB-Farbraum (falls vorhanden).
    /// </summary>
    /// <value></value>
    public Maybe<FileInfo> FileInfoAdobeRgb { get; private set; }

    public void AddAdobeRgbFileInfo(FileInfo fileInfo) => FileInfoAdobeRgb = Maybe<FileInfo>.From(fileInfo);

    public void ClearAdobeRgbFileInfo() => FileInfoAdobeRgb = Maybe<FileInfo>.None;

    public bool IsTiffOrPng => FileInfo.Extension.Equals(".tiff", StringComparison.InvariantCultureIgnoreCase) || 
        FileInfo.Extension.Equals(".tif", StringComparison.InvariantCultureIgnoreCase) ||
        FileInfo.Extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase);

    public bool IsJpeg => FileInfo.Extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase) || 
        FileInfo.Extension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase);


    private SupportedImage(FileInfo fileInfo)
    {
        FileInfo = fileInfo;
    }

    public static Result<SupportedImage> Create(FileInfo fileInfo, FileInfo fileInfoAdobeRgb)
    {
        var supportedImage = new SupportedImage(fileInfo);
        supportedImage.AddAdobeRgbFileInfo(fileInfoAdobeRgb);
        return Result.Success(supportedImage);
    }

    public static Result<SupportedImage> Create(FileInfo fileInfo)
    {
        // Prüfe, einschliesslich Gross- und Kleinschreibung (InvariantCultureIgnoreCase), ob die Dateiendung .jpg, .jpeg oder .png ist
        if (IsSupportedImageExtension(fileInfo))
        {
            return new SupportedImage(fileInfo);
        }

        return Result.Failure<SupportedImage>($"File {fileInfo.FullName} is not a supported cover art image.");
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

    public static bool IsPngFileExtension(FileInfo fileInfo)
    {
        return fileInfo.Extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool IsJpgFileExtension(FileInfo fileInfo)
    {
        return fileInfo.Extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase) || fileInfo.Extension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase);
    }

    public static FileInfo GetJpgFileName(FileInfo pngFileInfo)
    {
        return new FileInfo(pngFileInfo.FullName.Replace(".png", ".jpg"));
    }


    public override string ToString() => FileInfo.FullName;

    public static implicit operator FileInfo(SupportedImage supportedImage) => supportedImage.FileInfo;
    public static implicit operator string(SupportedImage supportedImage) => supportedImage.FileInfo.FullName;

    public void UpdateFilePath(string imageFileTargetPath) => FileInfo = new FileInfo(imageFileTargetPath);

    public void UpdateFilePathAdobeRgb(string newFilePathAdobeRgb) => FileInfoAdobeRgb = Maybe<FileInfo>.From(new FileInfo(newFilePathAdobeRgb));

    public static Result<List<SupportedImage>> GetSupportedImagesFromDirectory(DirectoryInfo directoryInfo)
    {
        if (!directoryInfo.Exists)
        {
            return Result.Failure<List<SupportedImage>>($"The directory {directoryInfo.FullName} does not exist.");
        }

        // verwende ParsePngJpgPair, um die Paare von PNG- und JPG-Dateien zu ermitteln
        var supportedImageFiles = directoryInfo.GetFiles().Where(f => IsSupportedImageExtension(f)).ToList();
        var supportedImagesPaired = ParsePngJpgPair(supportedImageFiles);
        if (supportedImagesPaired.IsFailure)
        {
            return Result.Failure<List<SupportedImage>>($"Error parsing PNG-JPG pairs: {supportedImagesPaired.Error}");
        }

        return supportedImagesPaired;
    }

    /// <summary>
    /// Ein SupportedImage-Objekt hat eine FileInfo und eine FileInfoAdobeRgb-Eigenschaft. Die Aufgabe dieser Methode besteht aus den Paaren von JPG und PNG-Dateien,
    /// die den gleichen Dateinamen haben, zu ermitteln und in einer Liste von SupportedImage-Objekten zurückzugeben.
    /// Wenn bei jedem PNG ein passendes JPG gefunden wird, wird ein SupportedImage-Objekt erstellt und in die Liste aufgenommen.
    /// Damit wird die Liste der SupportedImages in der Regel halb so lang wie die Anzahl der PNG- und JPG-Dateien.
    /// </summary>
    /// <param name="imagesFailes"></param>
    /// <returns></returns>
    private static Result<List<SupportedImage>> ParsePngJpgPair(IEnumerable<FileInfo> imagesFailes)
    {
        var supportedImages = new List<SupportedImage>();

        // Nimm die PNG-Dateien als Ausgangslage und suche für jede PNG-Datei ein passendes JPG-Datei
        var pngFiles = imagesFailes.Where(f => IsPngFileExtension(f)).ToList();

        foreach (var pngFile in pngFiles)
        {
            var jpgFile = imagesFailes.FirstOrDefault(f => IsJpgFileExtension(f) && f.Name.Equals(GetJpgFileName(pngFile).Name, StringComparison.InvariantCultureIgnoreCase));
            if (jpgFile != null)
            {
                var supportedImage = Create(pngFile, jpgFile);
                if (supportedImage.IsSuccess)
                {
                    supportedImages.Add(supportedImage.Value);
                }
            }
        }

        return Result.Success(supportedImages);
    }
}