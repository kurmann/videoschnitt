using System.Xml.Linq;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.CommonServices.FileSystem.Unix;

public class FileOperations : IFileOperations
{
    private readonly ExecuteCommandService _executeCommandService;

    public FileOperations(ExecuteCommandService executeCommandService)
    {
        _executeCommandService = executeCommandService;
    }

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

    public async Task<Result> MoveFile(string sourcePath, string destinationPath, bool overwrite = false, bool inheritPermissions = true)
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

    public async Task<Result> CopyFile(string sourcePath, string destinationPath, bool overwrite = false, bool inheritPermissions = true)
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