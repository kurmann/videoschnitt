using CSharpFunctionalExtensions;
using Kurmann.Videoschnitt.ConfigurationModule.Settings;
using Microsoft.Extensions.Options;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.RemoteIntegration;

/// <summary>
/// Verantwortlich für das Löschen von Dateien oder leeren Verzeichnissen, die nicht mehr benötigt werden.
/// </summary>
public class DirectoryCleanupService
{
    private readonly InfuseMediaLibrarySettings _infuseMediaLibrarySettings;

    public DirectoryCleanupService(IOptions<InfuseMediaLibrarySettings> infuseMediaLibrarySettings)
    {
        _infuseMediaLibrarySettings = infuseMediaLibrarySettings.Value;
    }

    internal Result CleanupFiles()
    {
        // Lösche alle leeren Verzeichnisse im Infuse-Mediathek-Verzeichnis
        var removeEmptyDirectoriesResult = RemoveEmptyDirectories(_infuseMediaLibrarySettings.InfuseMediaLibraryPathLocal);
        if (removeEmptyDirectoriesResult.IsFailure)
        {
            return Result.Failure("Fehler beim Löschen von leeren Verzeichnissen: " + removeEmptyDirectoriesResult.Error);
        }

        // Wenn das lokale Infuse-Mediathek-Verzeichnis leer ist, lösche es
        var removeInfuseMediaLibraryPathLocalResult = RemoveInfuseMediaLibraryPathLocal(_infuseMediaLibrarySettings.InfuseMediaLibraryPathLocal);
        if (removeInfuseMediaLibraryPathLocalResult.IsFailure)
        {
            return Result.Failure("Fehler beim Löschen des lokalen Infuse-Mediathek-Verzeichnisses: " + removeInfuseMediaLibraryPathLocalResult.Error);
        }

        return Result.Success();
    }

    private Result RemoveInfuseMediaLibraryPathLocal(string infuseMediaLibraryPathLocal)
    {
        if (!Directory.Exists(infuseMediaLibraryPathLocal))
        {
            return Result.Success();
        }

        var files = GetNonHiddenFiles(new DirectoryInfo(infuseMediaLibraryPathLocal));

        if (!files.Any() && !Directory.EnumerateDirectories(infuseMediaLibraryPathLocal).Any())
        {
            // Lösche das Verzeichnis
            Directory.Delete(infuseMediaLibraryPathLocal, true);
        }

        return Result.Success();
    }

    private Result RemoveEmptyDirectories(string path)
    {
        try
        {
            RemoveEmptyDirectoriesRecursive(new DirectoryInfo(path));
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Fehler beim Löschen von leeren Verzeichnissen: {ex.Message}");
        }
    }

    private void RemoveEmptyDirectoriesRecursive(DirectoryInfo directory)
    {
        foreach (var subDirectory in directory.GetDirectories())
        {
            RemoveEmptyDirectoriesRecursive(subDirectory);
        }

        if (!directory.EnumerateFileSystemInfos().Any())
        {
            directory.Delete();
        }
    }

    private static IEnumerable<FileInfo> GetNonHiddenFiles(DirectoryInfo directory)
    {
        // ignoriere versteckte Dateien, bspw. .DS_Store
        return directory.GetFiles().Where(file => !file.Attributes.HasFlag(FileAttributes.Hidden) && !file.Name.StartsWith("."));
    }
}