using System.Xml.Linq;
using Kurmann.Videoschnitt.CommonServices;
using Kurmann.Videoschnitt.CommonServices.FileSystem;

namespace Kurmann.Videoschnitt.LocalFileSystem.FileSystem.Unix;

public class FileOperations : IFileOperations
{
    private readonly ExecuteCommandService _executeCommandService;

    public FileOperations(ExecuteCommandService executeCommandService)
    {
        _executeCommandService = executeCommandService;
    }

    public async void SaveFile(XDocument document, string path, bool inheritPermissions = true)
    {
        document.Save(path);
        if (inheritPermissions)
        {
            await ResetPermissionsToInherit(path);
        }
    }

    public async void MoveFile(string sourcePath, string destinationPath, bool overwrite = false, bool inheritPermissions = true)
    {
        if (overwrite && File.Exists(destinationPath))
        {
            File.Delete(destinationPath);
        }

        File.Move(sourcePath, destinationPath);

        if (inheritPermissions)
        {
            await ResetPermissionsToInherit(destinationPath);
        }
    }

    public async void CopyFile(string sourcePath, string destinationPath, bool overwrite = false, bool inheritPermissions = true)
    {
        File.Copy(sourcePath, destinationPath, overwrite);

        if (inheritPermissions)
        {
            await ResetPermissionsToInherit(destinationPath);
        }
    }

    public string ReadFile(string path)
    {
        return File.ReadAllText(path);
    }

    private async Task ResetPermissionsToInherit(string path)
    {
        var chmodResult = await _executeCommandService.ExecuteCommandAsync("chmod", $"u+rX,g+rX,o+rX \"{path}\"");
        if (chmodResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to reset permissions to inherit for {path}: {chmodResult.Error}");
        }
    }
}