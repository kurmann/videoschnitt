using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.CommonServices
{
    /// <summary>
    /// Service zum Kopieren, Verschieben und Lesen von Dateien.
    /// Dieser Service tritt an die Stelle von System.IO.File, um die Berechtigungen beim Kopieren und Verschieben zu übernehmen.
    /// .NET Core bietet keine Möglichkeit, die Berechtigungen beim Kopieren und Verschieben von Dateien zu übernehmen,
    /// da .NET Core als plattformübergreifendes Framework keine Windows-spezifischen Funktionen unterstützt.
    /// Dieser Service verwendet daher die Unix-Befehle "cp" und "mv" und führt sie in einem externen Prozess aus.
    /// </summary>
    public class FileTransferService
    {
        private readonly ILogger<FileTransferService> _logger;
        private readonly ExecuteCommandService _executeCommandService;

        public FileTransferService(ILogger<FileTransferService> logger, ExecuteCommandService executeCommandService)
        {
            _logger = logger;
            _executeCommandService = executeCommandService;
        }

        /// <summary>
        /// Kopiert eine Datei und übernimmt die Berechtigungen.
        /// </summary>
        /// <param name="sourcePath">Der Quellpfad der Datei.</param>
        /// <param name="destinationPath">Der Zielpfad der Datei.</param>
        /// <returns>Ein Result-Objekt, das den Erfolg oder Fehler enthält.</returns>
        public async Task<Result> CopyFileWithPermissionsAsync(string sourcePath, string destinationPath)
        {
            var commandPath = "cp";
            var arguments = $"-p \"{sourcePath}\" \"{destinationPath}\"";
            return await _executeCommandService.ExecuteCommandAsync(commandPath, arguments);
        }

        /// <summary>
        /// Verschiebt eine Datei und übernimmt die Berechtigungen.
        /// </summary>
        /// <param name="sourcePath">Der Quellpfad der Datei.</param>
        /// <param name="destinationPath">Der Zielpfad der Datei.</param>
        /// <returns>Ein Result-Objekt, das den Erfolg oder Fehler enthält.</returns>
        public async Task<Result> MoveFileWithPermissionsAsync(string sourcePath, string destinationPath)
        {
            var commandPath = "mv";
            var arguments = $"\"{sourcePath}\" \"{destinationPath}\"";
            return await _executeCommandService.ExecuteCommandAsync(commandPath, arguments);
        }

        /// <summary>
        /// Liest den Inhalt einer Datei.
        /// </summary>
        /// <param name="filePath">Der Pfad der Datei.</param>
        /// <returns>Ein Result-Objekt, das den Erfolg oder Fehler enthält.</returns>
        public async Task<Result<string>> ReadFileAsync(string filePath)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                return Result.Success(content);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fehler beim Lesen der Datei {filePath}: {ex.Message}");
                return Result.Failure<string>($"Fehler beim Lesen der Datei {filePath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Schreibt den Inhalt in eine Datei.
        /// </summary>
        /// <param name="content">Der Inhalt, der geschrieben werden soll.</param>
        /// <param name="filePath">Der Pfad der Datei.</param>
        /// <returns>Ein Result-Objekt, das den Erfolg oder Fehler enthält.</returns>
        public async Task<Result> WriteFileAsync(string content, string filePath)
        {
            var commandPath = "sh";
            var arguments = $"-c 'echo \"{content.Replace("\"", "\\\"")}\" > \"{filePath}\"'";
            return await _executeCommandService.ExecuteCommandAsync(commandPath, arguments);
        }
    }
}