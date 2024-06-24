namespace Kurmann.Videoschnitt.Common.Models;

/// <summary>
/// Repräsentiert eine Masterdatei.
/// Sie ist die Videodatei in der höchsten Qualität und als Ausgangslage für alle weiteren Prozesse.
/// </summary>
public record Masterfile(FileInfo FileInfo, string Codec, string? Profile);