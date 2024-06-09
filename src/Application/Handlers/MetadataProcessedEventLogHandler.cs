using Microsoft.AspNetCore.SignalR;
using Kurmann.Videoschnitt.Messages.Metadata;

namespace Kurmann.Videoschnitt.Application;

public class MetadataProcessedEventLogHandler(IHubContext<LogHub> logHubContext)
{
    public async Task Handle(MetadataProcessedEvent message)
    {
        var logMessage = $"Metadaten für das Verzeichnis '{message.InputDirectory}' wurden erfolgreich verarbeitet.";
        await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);

        // Liste die Dateinamen der verarbeiteten Medien-Dateien auf
        if (message.ProcessedFiles == null || message.ProcessedFiles.Count == 0)
        {
            logMessage = "Keine Medien-Dateien verarbeitet.";
            await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);
            return;
        }

        foreach (var processedFile in message.ProcessedFiles)
        {
            logMessage = $"Verarbeitete Medien-Datei: {processedFile.Name}";
            await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);
        }

        Console.WriteLine(logMessage);
    }
}
