using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kurmann.Videoschnitt.MetadataProcessor.Entities;

/// <summary>
/// Repräsentiert eine Sammlung von Mediensets.
/// Die Entität übernimmt die Gruppierung von Medien-Dateien in Mediensets.
/// Ein einzelnes Medienset ist eine Gruppe von Medien-Dateien, die zusammengehören.
/// So kann von einem Familienvideo mehrere Videoversionen existieren (QuickTime MOV für Medienserver und MP4 für die Cloud).
/// Ein Medienset wird erkannt durch folgendes Format im Dateinamen: "AufnahmedatumIso Titel(-Variante)".
/// Die Variante ist optional und wird durch einen Bindestrich getrennt.
/// </summary>
/// <remarks>
/// Beispiel für ein Medienset
/// 2024-06-05 Zeitraffer Wolken Testaufnahme-4K-Internet.m4v
/// 2024-06-05 Zeitraffer Wolken Testaufnahme-4K-Medienserver.mov
/// 2024-06-05 Zeitraffer Wolken Testaufnahme-1080p-Internet.m4v
/// 2024-06-05 Zeitraffer Wolken Testaufnahme.jpg
/// </remarks>
public class MediasetCollection
{
    public List<FileInfo> IgnoredFiles { get; }

    private MediasetCollection(List<FileInfo> ignoredFiles)
    {
        IgnoredFiles = ignoredFiles;
    }

    public static MediasetCollection Create(IEnumerable<FileInfo> mediaFiles,
                                            List<string> videoVersionSuffixes,
                                            List<string> imageVersionSuffixes,
                                            List<string> supportedVideoExtensions,
                                            List<string> supportedImageExtensions)
    {
        // Ermittele alle Medien-Dateien, die mit einem ISO-Datum beginnen
        var filesWithoutLeadingIsoDate = GetFilesWithoutLeadingIsoDate(mediaFiles);
        var mediaFilesWithLeadingIsoDate = mediaFiles.Except(filesWithoutLeadingIsoDate).ToList();

        // Ermittle alle unterstützten Video-Dateien
        var supportedVideoFiles = GetSupportedVideoFiles(mediaFilesWithLeadingIsoDate, supportedVideoExtensions);

        // Gruppiere die Videodateien nach Medienset. Eine Videogruppe sind alle Videodateien, die sich nur im Varianten-Suffix unterscheiden.
        // Also konkret, alle Videodateien mit identischem Dateiennamen, wenn man das Varianten-Suffix ignoriert.
        var videoMediasets = supportedVideoFiles.GroupBy(f => GetFileNameWithoutVersionSuffix(f, videoVersionSuffixes)).ToList();

        // Ermittle alle unterstützten Bild-Dateien
        var supportedImageFiles = GetSupportedImageFiles(mediaFilesWithLeadingIsoDate, supportedImageExtensions);

        // Gruppiere die Bilddateien nach Medienset. Eine Bildgruppe sind alle Bilddateien, die sich nur im Varianten-Suffix unterscheiden.
        // Also konkret, alle Bilddateien mit identischem Dateiennamen, wenn man das Varianten-Suffix ignoriert.
        var imageMediasets = supportedImageFiles.GroupBy(f => GetFileNameWithoutVersionSuffix(f, imageVersionSuffixes)).ToList();

        // Füge die Video- und Bild-Mediensets zusammen indem alle Dateien mit dem gleichen Gruppenschlüssel (Dateiname ohne Varianten-Suffix) zusammengefasst werden.
        var mediasets = new List<MediaSet>();
        foreach (var videoMediaset in videoMediasets)
        {
            var mediaset = new MediaSet
            {
                Name = videoMediaset.Key,
                Videos = videoMediaset.ToList(),
                Images = imageMediasets.FirstOrDefault(i => i.Key == videoMediaset.Key)?.ToList() ?? new List<FileInfo>()
            };
            mediasets.Add(mediaset);
        }

        return new MediasetCollection(filesWithoutLeadingIsoDate);
    }

    // Gib eine Liste von Medien-Dateien zurück, die ohne ISO-Datum und nachfolgendem Leerzeichen beginnen
    private static List<FileInfo> GetFilesWithoutLeadingIsoDate(IEnumerable<FileInfo>? mediaFiles)
    {
        // Wenn keine Mediendatein vorhanden sind, gebe eine leere Liste zurück
        if (mediaFiles == null || !mediaFiles.Any())
        {
            return new List<FileInfo>();
        }

        // Entferne Dateien, die ohne ISO-Datum und nachfolgendem Leerzeichen beginnen
        var filesWithoutLeadingIsoDate = mediaFiles.Where(f => !f.Name.StartsWith("20") && f.Name.Contains(" ")).ToList();
        return filesWithoutLeadingIsoDate;
    }

    /// <summary>
    /// Entferne alle Varianten-Suffixe aus dem Dateinamen, einschließlich der Dateiendung.
    /// </summary>
    private static string GetFileNameWithoutVersionSuffix(FileInfo mediaFile, List<string> versionSuffixes)
    {
        var fileName = mediaFile.Name;
        foreach (var versionSuffix in versionSuffixes)
        {
            // Nimm den Dateinamen ohne Dateiendung
            fileName = Path.GetFileNameWithoutExtension(fileName);

            // Prüfe ob der Dateiname mit dem Varianten-Suffix endet (exklusive Dateiendung), wenn ja, entferne den Suffix
            if (fileName.EndsWith(versionSuffix) && fileName.Length > versionSuffix.Length + 1)
            {
                fileName = fileName.Substring(0, fileName.LastIndexOf(versionSuffix));
            }
        }

        return fileName;
    }

    private static List<FileInfo> GetSupportedVideoFiles(IEnumerable<FileInfo> mediaFiles, List<string> supportedVideoExtensions)
    {
        return mediaFiles.Where(f => supportedVideoExtensions.Contains(f.Extension)).ToList();
    }

    private static List<FileInfo> GetSupportedImageFiles(IEnumerable<FileInfo> mediaFiles, List<string> supportedImageExtensions)
    {
        return mediaFiles.Where(f => supportedImageExtensions.Contains(f.Extension)).ToList();
    }
}

internal record MediaSet
{
    public string? Name { get; set; }

    internal List<FileInfo> Videos { get; set; } = new List<FileInfo>();

    internal List<FileInfo> Images { get; set; } = new List<FileInfo>();
}