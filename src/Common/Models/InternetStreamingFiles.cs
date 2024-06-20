using Kurmann.Videoschnitt.Common.Entities.MediaTypes;

namespace Kurmann.Videoschnitt.Common.Models;

/// <summary>
/// Repräsentiert die Dateien eines Mediensets für das Internetwiedergabe als Teil eines Mediensets.
/// </summary>
/// <param name="DirectoryInfo"></param>
/// <param name="MediaSetTitle"></param>
/// <param name="ImageFiles"></param>
/// <param name="VideoFiles"></param>
/// <returns></returns>
public record InternetStreamingFiles(IEnumerable<SupportedImage> ImageFiles, IEnumerable<SupportedVideo> VideoFiles);