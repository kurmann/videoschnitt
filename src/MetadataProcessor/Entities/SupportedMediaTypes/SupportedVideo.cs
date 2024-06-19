using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Entities.SupportedMediaTypes;

/// <summary>
/// Definiert ein unterstützte Video-Datei.
/// </summary>
public class SupportedVideo
{
    public FileInfo FileInfo { get; }

    private SupportedVideo(FileInfo fileInfo) => FileInfo = fileInfo;

    public static Result<SupportedVideo> Create(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Result.Failure<SupportedVideo>("The file path must not be empty.");
        }

        try
        {
            var fileInfo = new FileInfo(filePath);
            return Create(fileInfo);
        }
        catch (Exception ex)
        {
            return Result.Failure<SupportedVideo>($"Error creating the SupportedVideoType object: {ex.Message}");
        }
    }

    public static Result<SupportedVideo> Create(FileInfo fileInfo)
    {
        // Check if the file exists
        if (!fileInfo.Exists)
        {
            return Result.Failure<SupportedVideo>($"The file {fileInfo.FullName} does not exist.");
        }

        // Check if the file extension is correct
        if (!IsSupportedVideoExtension(fileInfo))
        {
            return Result.Failure<SupportedVideo>($"The file extension {fileInfo.Extension} is not a supported video type.");
        }

        return new SupportedVideo(fileInfo);
    }

    public static bool IsSupportedVideoExtension(FileInfo fileInfo)
    {
        return QuickTimeMovie.IsQuickTimeMovieExtension(fileInfo) || Mpeg4Video.IsVideoExtensionMpeg4(fileInfo);
    }
}