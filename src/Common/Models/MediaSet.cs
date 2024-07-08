using System.Security.Cryptography.X509Certificates;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Common.Models;

/// <summary>
/// Repräsentiert ein Medienset-Verzeichnis mit separierten Dateien für den Einsatzzweck auf einem lokalen Medienserver und im Internet.
/// Die jeweiligen Dateien für einen Einsatzzweck können auch leer sein, da nicht alle Mediensets für beide Einsatzzwecke konfiguriert sind
/// oder sich gewisse Dateien beim Verarbeitungsprozess noch nicht im Medienset-Verzeichnis befinden (bspw. während der Videokomprimierung).
/// </summary>
/// <param name="Title"></param>
/// <param name="LocalMediaServerFiles"></param>
/// <param name="InternetStreaming"></param>
/// <returns></returns>
public record MediaSet
{
    public MediaSet(string? title, Maybe<LocalMediaServerFiles> localMediaServerFiles, Maybe<InternetStreamingFiles> internetStreaming)
    {
        Title = title;
        LocalMediaServerFiles = localMediaServerFiles;
        InternetStreaming = internetStreaming;
    }

    public string? Title { get; init; }
    public Maybe<LocalMediaServerFiles> LocalMediaServerFiles { get; init; }
    public Maybe<InternetStreamingFiles> InternetStreaming { get; init; }
}

