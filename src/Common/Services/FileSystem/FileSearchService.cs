
namespace Kurmann.Videoschnitt.Common.Services.FileSystem;

public class FileSearchService : IFileSearchService
{
    public async IAsyncEnumerable<FileInfo> GetFilesAsync(IEnumerable<DirectoryInfo> directories, SearchOption searchOption)
    {
        await foreach (var file in GetFilesAsync(directories, "*.*", searchOption))
        {
            yield return file;
        }
    }

    public async IAsyncEnumerable<FileInfo> GetFilesAsync(IEnumerable<DirectoryInfo> directories, string searchPattern, SearchOption searchOption)
    {
        foreach (var directory in directories)
        {
            if (directory.Exists)
            {
                var files = await Task.Run(() => directory.EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly));
                foreach (var file in files)
                {
                    yield return file;
                }

                if (searchOption == SearchOption.AllDirectories)
                {
                    var subDirectories = directory.EnumerateDirectories();
                    await foreach (var file in GetFilesAsync(subDirectories, searchPattern, searchOption))
                    {
                        yield return file;
                    }
                }
            }
        }
    }
}