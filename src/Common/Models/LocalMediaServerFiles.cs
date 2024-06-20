using Kurmann.Videoschnitt.Common.Entities.MediaTypes;

namespace Kurmann.Videoschnitt.LocalFileSystem.Models;

/// <summary>
/// Repräsentiert die Dateien eines Mediensets für die Wiedergabe über den lokalen Medienserver.
/// Hinweis: Auf dem lokalen Medienserver wird nur ein Video unterstützt.
/// Bilddateien können mehrere sein, da unterschiedliche Bilddateien als Bannerbild und als Thumbnail verwendet werden können.
/// </summary>
/// <param name="DirectoryInfo"></param>
/// <param name="MediaSetTitle"></param>
/// <param name="ImageFiles"></param>
/// <param name="VideoFile"></param>
/// <returns></returns>
public record LocalMediaServerFiles(IEnumerable<SupportedImage> ImageFiles, SupportedVideo VideoFile);
