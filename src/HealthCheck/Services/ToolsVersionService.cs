using System;
using System.Diagnostics;
using System.Text;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.HealthCheck.Services;

public class ToolsVersionService
{
    public Result<string> GetFFmpegVersion()
    {
        try
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"ffmpeg -version\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(ex.Message);
        }
    }
}