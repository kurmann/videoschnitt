using System.Diagnostics;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.MetadataProcessor.Services;

public class FFmpegMetadataService
{
    public Result<string> GetFFmpegMetadata(string filePath)
    {
        try
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"ffmpeg -i '{filePath}' -f ffmetadata -\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(error))
            {
                return Result.Failure<string>(error);
            }

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(ex.Message);
        }
    }
}