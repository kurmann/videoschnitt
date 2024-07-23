using System;
using System.IO;

namespace Kurmann.Videoschnitt.ConfigurationModule.Settings
{
    public class ApplicationSettings
    {
        public const string SectionName = "Application";

        public string InfuseMediaLibraryPath { get; set; } = ExpandHomeDirectory(DefaultInfuseMediaLibraryPath);
        public const string InfuseMediaLibraryPathConfigKey = $"{SectionName}:InfuseMediaLibraryPath";
        public const string DefaultInfuseMediaLibraryPath = "~/Movies/Infuse Media Library";
        public bool IsDefaultInfuseMediaLibraryPath => InfuseMediaLibraryPath == DefaultInfuseMediaLibraryPath;

        [Obsolete("Use MediaSetOrganizerSettings.InputDirectory instead")]
        public string InputDirectory { get; set; } = ExpandHomeDirectory(DefaultInputDirectory);
        [Obsolete("Use MediaSetOrganizerSettings.InputDirectoryConfigKey instead")]
        public const string InputDirectoryConfigKey = $"{SectionName}:InputDirectory";
        [Obsolete("Use MediaSetOrganizerSettings.DefaultInputDirectory instead")]
        public const string DefaultInputDirectory = "~/Movies/Final Cut Export";
        [Obsolete("Use MediaSetOrganizerSettings.IsDefaultInputDirectory instead")]
        public bool IsDefaultInputDirectory => InputDirectory == DefaultInputDirectory;

        public string MediaSetPathLocal { get; set; } = ExpandHomeDirectory(DefaultMediaSetPathLocal);
        public const string MediaSetPathLocalConfigKey = $"{SectionName}:MediaSetPathLocal";
        public const string DefaultMediaSetPathLocal = "~/Movies/MediaSets";
        public bool IsDefaultMediaSetPathLocal => MediaSetPathLocal == DefaultMediaSetPathLocal;

        public string FinalCutExportDirectory { get; set; } = ExpandHomeDirectory(DefaultFinalCutExportDirectory);
        public const string FinalCutExportDirectoryConfigKey = $"{SectionName}:FinalCutExportDirectory";
        public const string DefaultFinalCutExportDirectory = "~/Movies/Final Cut Export";
        public bool IsDefaultFinalCutExportDirectory => FinalCutExportDirectory == DefaultFinalCutExportDirectory;

        public string InfuseMediaLibraryPathLocal { get; set; } = ExpandHomeDirectory(DefaultInfuseMediaLibraryPathLocal);
        public const string InfuseMediaLibraryPathLocalConfigKey = $"{SectionName}:InfuseMediaLibraryPathLocal";
        public const string DefaultInfuseMediaLibraryPathLocal = "~/Movies/Infuse Media Library";
        public bool IsDefaultInfuseMediaLibraryPathLocal => InfuseMediaLibraryPathLocal == DefaultInfuseMediaLibraryPathLocal;

        public string? MediaSetPathRemote { get; set; }
        public const string MediaSetPathRemoteConfigKey = $"{SectionName}:MediaSetPathRemote";

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