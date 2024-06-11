using Kurmann.Videoschnitt.Messages.MediaFiles;
using Microsoft.AspNetCore.SignalR;

namespace Kurmann.Videoschnitt.Application.Handlers;

public class MetadataProcessingHandlers(IHubContext<LogHub> logHubContext)
{
    public async Task Handle(MediaFilesForMetadataProcessingFoundEvent message)
    {
        await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", "Unterstützte Medien-Dateien für die Metadaten-Verarbeitung gefunden.");
        foreach (var mediaFile in message.MediaFiles)
        {
            await logHubContext.Clients.All.SendAsync("ReceiveLogMessage", $"- {mediaFile.Name}");
        }
    }
}