using System.Xml.Linq;

namespace Kurmann.Videoschnitt.CommonServices.FileSystem;

public class FileOperations : IFileOperations
{
    public void SaveFile(XDocument document, string path)
    {
        document.Save(path);
    }

    public void MoveFile(string sourcePath, string destinationPath, bool overwrite = false)
    {
        if (File.Exists(destinationPath))
        {
            File.Delete(destinationPath);
        }
        File.Move(sourcePath, destinationPath);
    }

    public void CopyFile(string sourcePath, string destinationPath, bool overwrite = false)
    {
        File.Copy(sourcePath, destinationPath, overwrite);
    }

    public string ReadFile(string path)
    {
        return File.ReadAllText(path);
    }
}