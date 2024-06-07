using Kurmann.Videoschnitt.Features.MetadataProcessor.Events;

namespace Kurmann.Videoschnitt.Application;

public class ProcessMetadataRequestHandler
{
    private readonly LogHub _logHub;

    public ProcessMetadataRequestHandler(LogHub logHub)
    {
        _logHub = logHub;
    }

    public async Task Handle(ProcessMetadataRequest message)
    {
        var logMessage = "Anfrage zur Verarbeitung der Metadaten erhalten.";
        await _logHub.SendMessage(logMessage);
        Console.WriteLine(logMessage);
    }
}