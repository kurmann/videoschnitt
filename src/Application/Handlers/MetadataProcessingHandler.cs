using Kurmann.Videoschnitt.Messages.MediaFiles;
using Microsoft.AspNetCore.SignalR;

namespace Kurmann.Videoschnitt.Application.Handlers;

public class MetadataProcessingHandler(IHubContext<LogHub> logHubContext)
{
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
}