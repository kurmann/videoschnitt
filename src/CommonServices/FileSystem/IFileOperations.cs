using System.Xml.Linq;

namespace Kurmann.Videoschnitt.CommonServices.FileSystem;

public interface IFileOperations
{
    void SaveFile(XDocument document, string path);
    void MoveFile(string sourcePath, string destinationPath, bool overwrite = false);
    void CopyFile(string sourcePath, string destinationPath, bool overwrite = false);
    string ReadFile(string path);
}