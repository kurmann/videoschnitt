using System.Xml.Linq;

namespace Kurmann.Videoschnitt.CommonServices.FileSystem;

public interface IFileOperations
{
    void SaveFile(XDocument document, string path, bool inheritPermissions = true);
    void MoveFile(string sourcePath, string destinationPath, bool overwrite = false, bool inheritPermissions = true);
    void CopyFile(string sourcePath, string destinationPath, bool overwrite = false, bool inheritPermissions = true);
    string ReadFile(string path);
}