using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.LocalFileSystem.Models;

/// <summary>
/// Repräsentiert ein Medienset-Verzeichnis mit separierten Dateien für den Einsatzzweck auf einem lokalen Medienserver und im Internet.
/// Die jeweiligen Dateien für einen Einsatzzweck können auch leer sein, da nicht alle Mediensets für beide Einsatzzwecke konfiguriert sind
/// oder sich gewisse Dateien beim Verarbeitungsprozess noch nicht im Medienset-Verzeichnis befinden (bspw. während der Videokomprimierung).
/// </summary>
/// <param name="MediaSetDirectory"></param>
/// <param name="MediaSetTitle"></param>
/// <param name="LocalMediaServerFiles"></param>
/// <param name="InternetStreaming"></param>
/// <returns></returns>
public record MediaSet(string MediaSetTitle, Maybe<LocalMediaServerFiles> LocalMediaServerFiles, Maybe<InternetStreamingFiles> InternetStreaming);
