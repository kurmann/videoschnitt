using System.Xml.Linq;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Common.Services.FileSystem;

public interface IFileOperations
{
    Task<Result> SaveFile(XDocument document, string path, bool inheritPermissions = true);
    Task<Result> MoveFileAsync(string sourcePath, string destinationPath, bool overwrite = false, bool inheritPermissions = true);
    Task<Result> CopyFileAsync(string sourcePath, string destinationPath, bool overwrite = false, bool inheritPermissions = true);
    Result<string> ReadFile(string path);
    Task<Result> CreateDirectoryAsync(string path, bool inheritPermissions = true);
}