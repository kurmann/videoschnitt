namespace Kurmann.Videoschnitt.Common.Services.FileSystem;

public interface IFileSearchService
{
    IAsyncEnumerable<FileInfo> GetFilesAsync(IEnumerable<DirectoryInfo> directories, SearchOption searchOption);
    IAsyncEnumerable<FileInfo> GetFilesAsync(IEnumerable<DirectoryInfo> directories, string searchPattern, SearchOption searchOption);
    IAsyncEnumerable<FileInfo> GetFilesAsync(DirectoryInfo directory, SearchOption searchOption);
    IAsyncEnumerable<FileInfo> GetFilesAsync(DirectoryInfo directory, string searchPattern, SearchOption searchOption);
}