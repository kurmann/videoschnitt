using Kurmann.Videoschnitt.Messages.MediaFiles;
using Kurmann.Videoschnitt.Messages.Metadata;
using Microsoft.AspNetCore.SignalR;

namespace Kurmann.Videoschnitt.Application.Handlers;

public class MetadataProcessingHandler(IHubContext<LogHub> logHubContext)
{
    public async Task Handle(ProcessMetadataRequest _)
    {
        var logMessage = "Anfrage zur Verarbeitung der Metadaten erhalten.";
        await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);
        Console.WriteLine(logMessage);
    }

    public async Task Handle(MediaFilesForMetadataProcessingFoundEvent message)
    {
        await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", "Unterstützte Medien-Dateien für die Metadaten-Verarbeitung gefunden.");
        foreach (var mediaFile in message.MediaFiles)
        {
            await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", $"- {mediaFile.Name}");
        }
    }

    public async Task Handle(MediaFilesForMetadataProcessingFoundErrorEvent error)
    {
        await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", $"Fehler beim Abrufen der unterstützten Medien-Dateien für die Metadaten-Verarbeitung: {error.Error}");
    }

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