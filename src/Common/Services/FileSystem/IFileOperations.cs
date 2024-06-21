using System.Xml.Linq;
using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.Common.Services.FileSystem;

public interface IFileOperations
{
    Task<Result> SaveFile(XDocument document, string path, bool inheritPermissions = true);
    Task<Result> MoveFile(string sourcePath, string destinationPath, bool overwrite = false, bool inheritPermissions = true);
    Task<Result> CopyFile(string sourcePath, string destinationPath, bool overwrite = false, bool inheritPermissions = true);
    Result<string> ReadFile(string path);
}