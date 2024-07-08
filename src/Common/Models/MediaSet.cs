using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.Common.Entities.MediaTypes;

namespace Kurmann.Videoschnitt.Common.Models;

/// <summary>
/// Repräsentiert ein Medienset-Verzeichnis mit separierten Dateien für den Einsatzzweck auf einem lokalen Medienserver und im Internet.
/// Die jeweiligen Dateien für einen Einsatzzweck können auch leer sein, da nicht alle Mediensets für beide Einsatzzwecke konfiguriert sind
/// oder sich gewisse Dateien beim Verarbeitungsprozess noch nicht im Medienset-Verzeichnis befinden (bspw. während der Videokomprimierung).
/// </summary>
/// <returns></returns>
public record MediaSet
{
    public string? Title { get; init; }

    public Maybe<SupportedVideo> LocalMediaServerVideoFile { get; init;}

    public Maybe<List<SupportedVideo>> InternetStreamingVideoFiles { get; init; }

    public Maybe<List<SupportedImage>> ImageFiles { get; init; }

    public Maybe<Masterfile> Masterfile { get; set; }
}

