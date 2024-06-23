using System.Xml.Linq;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Common.Services.FileSystem.Unix;

/// <summary>
/// Stellt Methoden zum Arbeiten mit Dateien auf Unix-Systemen bereit.
/// </summary>
public class FileOperations : IFileOperations
{
    private readonly ExecuteCommandService _executeCommandService;

    public FileOperations(ExecuteCommandService executeCommandService) => _executeCommandService = executeCommandService;

    /// <summary>
    /// Speichert das XDocument-Dokument an dem angegebenen Pfad. Bietet gegenüber dem Speichern mit XDocument.Save() die Möglichkeit, die Berechtigungen des übergeordneten Ordners zu erben.
    /// Dies ist insbesondere auf Unix-Systemen wichtig, da .NET Core keine Möglichkeit bietet, die Berechtigungen beim Speichern von Dateien zu übernehmen.
    /// </summary>
    /// <param name="document">Das XDocument-Dokument, das gespeichert werden soll.</param>
    /// <param name="path">Der Pfad, an dem das Dokument gespeichert werden soll.</param>
    /// <param name="inheritPermissions">Gibt an, ob die Berechtigungen des übergeordneten Ordners geerbt werden sollen. Standardmäßig ist dies true.</param>
    /// <returns>Ein Result-Objekt, das den Erfolg oder Misserfolg des Speicherns angibt.</returns>
    public async Task<Result> SaveFile(XDocument document, string path, bool inheritPermissions = true)
    {
        try
        {
            document.Save(path);
            if (inheritPermissions)
            {
                var resetResult = await ResetPermissionsToInherit(path);
                if (resetResult.IsFailure)
                {
                    return resetResult;
                }
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Fehler beim Speichern der Datei: {ex.Message}");
        }
    }

    /// <summary>
    /// Verschiebt eine Datei vom Quellpfad zum Zielort. Bietet gegenüber dem Verschieben mit File.Move() die Möglichkeit, die Berechtigungen zu erben.
    /// Dies ist insbesondere auf Unix-Systemen wichtig, da .NET Core keine Möglichkeit bietet, die Berechtigungen beim Verschieben von Dateien zu übernehmen.
    /// </summary>
    /// <param name="sourcePath">Der Pfad der zu verschiebenden Datei.</param>
    /// <param name="destinationPath">Der Zielort, an dem die Datei verschoben werden soll.</param>
    /// <param name="overwrite">Gibt an, ob die Zieldatei überschrieben werden soll, falls sie bereits existiert. Standardmäßig ist dies false.</param>
    /// <param name="inheritPermissions">Gibt an, ob die Berechtigungen von der Quelldatei auf die Zieldatei übernommen werden sollen. Standardmäßig ist dies true.</param>
    /// <returns>Ein <see cref="Task{Result}"/>, das die asynchrone Operation repräsentiert. Das Ergebnis der Aufgabe enthält ein <see cref="Result"/>, das den Erfolg oder Misserfolg der Operation angibt.</returns>
    public async Task<Result> MoveFileAsync(string sourcePath, string destinationPath, bool overwrite = false, bool inheritPermissions = true)
    {
        try
        {
            if (overwrite && File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            File.Move(sourcePath, destinationPath);

            if (inheritPermissions)
            {
                var resetResult = await ResetPermissionsToInherit(destinationPath);
                if (resetResult.IsFailure)
                {
                    return resetResult;
                }
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Fehler beim Verschieben der Datei: {ex.Message}");
        }
    }

    /// <summary>
    /// Kopiert eine Datei von einem Quellpfad zu einem Ziel- pfad. Bietet gegenüber dem Kopieren mit File.Copy() die Möglichkeit, die Berechtigungen zu erben.
    /// Dies ist insbesondere auf Unix-Systemen wichtig, da .NET Core keine Möglichkeit bietet, die Berechtigungen beim Kopieren von Dateien zu übernehmen.
    /// </summary>
    /// <param name="sourcePath">Der Quellpfad der Datei.</param>
    /// <param name="destinationPath">Der Zielpfad, an dem die Datei kopiert werden soll.</param>
    /// <param name="overwrite">Gibt an, ob eine vorhandene Datei überschrieben werden soll. Der Standardwert ist <c>false</c>.</param>
    /// <param name="inheritPermissions">Gibt an, ob die Berechtigungen der Ziel- datei von der Quelldatei geerbt werden sollen. Der Standardwert ist <c>true</c>.</param>
    /// <returns>Ein <see cref="Result"/>-Objekt, das den Erfolg oder Misserfolg des Kopiervorgangs darstellt.</returns>
    public async Task<Result> CopyFileAsync(string sourcePath, string destinationPath, bool overwrite = false, bool inheritPermissions = true)
    {
        try
        {
            File.Copy(sourcePath, destinationPath, overwrite);

            if (inheritPermissions)
            {
                var resetResult = await ResetPermissionsToInherit(destinationPath);
                if (resetResult.IsFailure)
                {
                    return resetResult;
                }
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Fehler beim Kopieren der Datei: {ex.Message}");
        }
    }

    /// <summary>
    /// Liest den Inhalt einer Datei.
    /// </summary>
    /// <param name="path">Der Pfad zur Datei.</param>
    /// <returns>Ein Result-Objekt, das den gelesenen Inhalt enthält, falls erfolgreich. Andernfalls enthält es eine Fehlermeldung.</returns>
    public Result<string> ReadFile(string path)
    {
        try
        {
            var content = File.ReadAllText(path);
            return Result.Success(content);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"Fehler beim Lesen der Datei: {ex.Message}");
        }
    }

    /// <summary>
    /// Erstellt ein Verzeichnis mit den angegebenen Berechtigungen.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="inheritPermissions"></param>
    /// <returns></returns>
    public async Task<Result> CreateDirectoryAsync(string path, bool inheritPermissions = true)
    {
        try
        {
            Directory.CreateDirectory(path);

            if (inheritPermissions)
            {
                var resetResult = await ResetPermissionsToInherit(path);
                if (resetResult.IsFailure)
                {
                    return resetResult;
                }
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Fehler beim Erstellen des Verzeichnisses: {ex.Message}");
        }
    }

    /// <summary>
    /// Gibt an, ob eine Datei verwendet wird. Verwendet das lsof-Tool, um zu überprüfen, ob die Datei von einem Prozess verwendet wird.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task<Result<bool>> IsFileInUseAsync(string path)
    {
        try
        {
            var lsofResult = await _executeCommandService.ExecuteBooleanCommandAsync("lsof", $"\"{path}\"");
            return Result.Success(lsofResult);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>($"Fehler beim Überprüfen der Datei: {ex.Message}");
        }
    }

    private async Task<Result> ResetPermissionsToInherit(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Result.Failure("Berechtigungen können nicht zurückgesetzt werden, da der Pfad leer ist.");
        }

        var chmodResult = await _executeCommandService.ExecuteCommandAsync("chmod", $"u+rX,g+rX,o+rX \"{path}\"");
        if (chmodResult.IsFailure)
        {
            return Result.Failure($"Fehler beim Zurücksetzen der Berechtigungen: {chmodResult.Error}");
        }

        return Result.Success();
    }
}