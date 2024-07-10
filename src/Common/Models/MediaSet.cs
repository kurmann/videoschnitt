using System.Reflection;
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

    public Maybe<List<SupportedImage>> ImageFiles { get; set; }

    public Maybe<Masterfile> Masterfile { get; set; }

    public bool IsNoImageFile => ImageFiles.HasNoValue || ImageFiles.Value.Count == 0;
    public bool IsSingleImageFile => ImageFiles.HasValue && ImageFiles.Value.Count == 1;
    public bool IsMultipleImageFiles => ImageFiles.HasValue && ImageFiles.Value.Count > 1;

    /// <summary>
    /// Gibt die Bilder sortiert nach dem letzten Änderungsdatum zurück.
    /// </summary>
    public List<SupportedImage> GetImagesOrderedByLastWriteTime()
    {
        return ImageFiles.Value.OrderByDescending(x => x.FileInfo.LastWriteTime).ToList();
    }

    /// <summary>
    /// Returns single image from the list of images.
    /// Throws an exception if there are more than one image in the list or no image at all.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public SupportedImage GetSingleImage()
    {
        if (IsNoImageFile)
        {
            throw new InvalidOperationException("No image file found in the media set.");
        }
        if (IsMultipleImageFiles)
        {
            throw new InvalidOperationException("More than two image files found in the media set.");
        }
        return ImageFiles.Value.Single();
    }
}

