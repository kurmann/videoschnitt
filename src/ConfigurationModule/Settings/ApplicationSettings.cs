using System;
using System.IO;

namespace Kurmann.Videoschnitt.ConfigurationModule.Settings
{
    public class ApplicationSettings
    {
        public const string SectionName = "Application";

        public string MediaSetPathLocal { get; set; } = ExpandHomeDirectory(DefaultMediaSetPathLocal);
        public const string MediaSetPathLocalConfigKey = $"{SectionName}:MediaSetPathLocal";
        public const string DefaultMediaSetPathLocal = "~/Movies/MediaSets";
        public bool IsDefaultMediaSetPathLocal => MediaSetPathLocal == DefaultMediaSetPathLocal;


        public ExternalToolsSettings ExternalTools { get; set; } = new ExternalToolsSettings();

        public static string ExpandHomeDirectory(string path)
        {
            if (path.StartsWith('~'))
            {
                var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return path.Replace("~", homeDirectory);
            }
            return path;
        }
    }

    public class ExternalToolsSettings
    {
        public FFMmpegSettings FFMpeg { get; set; } = new FFMmpegSettings();
        public FFProbeSettings FFProbe { get; set; } = new FFProbeSettings();
        public SipsSettings Sips { get; set; } = new SipsSettings();
    }

    public class FFMmpegSettings
    {
        public string Path { get; set; } = "/usr/local/bin/ffmpeg";
    }

    public class FFProbeSettings
    {
        public string Path { get; set; } = "/opt/local/bin/ffprobe";
    }

    public class SipsSettings
    {
        public string Path { get; set; } = "/usr/bin/sips";
    }
}